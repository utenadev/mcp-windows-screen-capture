using System.CommandLine;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;

[DllImport("user32.dll")] static extern bool SetProcessDPIAware();

var ipOption = new Option<string>(
    name: "--ip_addr",
    description: "IP address to bind (0.0.0.0 for WSL2, 127.0.0.1 for local only)",
    getDefaultValue: () => "127.0.0.1");

var portOption = new Option<int>(
    name: "--port",
    description: "Port number",
    getDefaultValue: () => 5000);

var desktopOption = new Option<uint>(
    name: "--desktopNum",
    description: "Default monitor index (0=primary)",
    getDefaultValue: () => 0u);

var httpOption = new Option<bool>(
    name: "--http",
    description: "Run in HTTP mode (requires ip/port options). Default is stdio mode.",
    getDefaultValue: () => false);

var rootCmd = new RootCommand("MCP Windows Screen Capture Server");
rootCmd.AddOption(ipOption);
rootCmd.AddOption(portOption);
rootCmd.AddOption(desktopOption);
rootCmd.AddOption(httpOption);

rootCmd.SetHandler((ip, port, desktop, useHttp) => {
    SetProcessDPIAware();
    
    var captureService = new ScreenCaptureService(desktop);
    captureService.InitializeMonitors();
    ScreenCaptureTools.SetCaptureService(captureService);
    
    if (useHttp) {
        RunHttpMode(ip, port, desktop, captureService);
    } else {
        RunStdioMode();
    }
}, ipOption, portOption, desktopOption, httpOption);

await rootCmd.InvokeAsync(args);

void RunStdioMode() {
    Console.Error.WriteLine("[Stdio] MCP Windows Screen Capture Server started in stdio mode");
    
    var builder = Host.CreateApplicationBuilder();
    builder.Services
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly(typeof(ScreenCaptureTools).Assembly);
    
    var host = builder.Build();
    host.Run();
}

void RunHttpMode(string ip, int port, uint desktop, ScreenCaptureService captureService) {
    var builder = WebApplication.CreateBuilder();
    builder.Services.AddSingleton<ScreenCaptureService>(sp => captureService);
    builder.WebHost.ConfigureKestrel(options => {
        options.Listen(IPAddress.Parse(ip), port);
    });
    
    var app = builder.Build();
    var service = app.Services.GetRequiredService<ScreenCaptureService>();
    
    var sessionManager = new McpSessionManager();
    var streamableHttp = new StreamableHttpServer(service, sessionManager);
    streamableHttp.Configure(app);
    
    app.MapGet("/stream/{id}", async (string id, HttpContext ctx) => {
        if (!service.TryGetSession(id, out var session) || session == null) return Results.NotFound();
        
        ctx.Response.ContentType = "text/event-stream";
        await foreach (var img in session.Channel.Reader.ReadAllAsync(ctx.RequestAborted)) {
            await ctx.Response.WriteAsync($"data: {img}\n\n");
            await ctx.Response.Body.FlushAsync();
        }
        return Results.Empty;
    });
    
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStopping.Register(() => {
        Console.WriteLine("[Server] Shutting down...");
        service.StopAllStreams();
        Console.WriteLine("[Server] Cleanup completed");
    });
    
    Console.CancelKeyPress += (sender, e) => {
        e.Cancel = true;
        Console.WriteLine("[Server] Ctrl+C pressed, initiating shutdown...");
        lifetime.StopApplication();
    };
    
    Console.WriteLine($"[Server] Started on http://{ip}:{port}");
    Console.WriteLine($"[Server] Default monitor: {desktop}");
    Console.WriteLine($"[Server] Streamable HTTP endpoint: http://{ip}:{port}/mcp (for Claude Code, Gemini CLI, OpenCode)");
    Console.WriteLine($"[Server] Stream endpoint: http://{ip}:{port}/stream/{{sessionId}}");
    if (ip == "0.0.0.0") {
        Console.WriteLine($"[Server] WSL2 HTTP URL: http://$(ip route | grep default | awk '{{print $3}}'):{port}/mcp");
        Console.WriteLine($"[Server] WSL2 Stream URL: http://$(ip route | grep default | awk '{{print $3}}'):{port}/stream/{{sessionId}}");
    }
    Console.WriteLine("[Server] Press Ctrl+C to stop");
    
    app.Run();
}
