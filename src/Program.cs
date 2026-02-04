using System.CommandLine;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using ModelContextProtocol.Server;

[DllImport("user32.dll")] static extern bool SetProcessDPIAware();

var desktopOption = new Option<uint>(
    name: "--desktopNum",
    description: "Default monitor index (0=primary)",
    getDefaultValue: () => 0u);

var rootCmd = new RootCommand("MCP Windows Screen Capture Server");
rootCmd.AddOption(desktopOption);

rootCmd.SetHandler((desktop) => {
    SetProcessDPIAware();
    
    var captureService = new ScreenCaptureService(desktop);
    captureService.InitializeMonitors();
    ScreenCaptureTools.SetCaptureService(captureService);
    
    Console.Error.WriteLine("[Stdio] MCP Windows Screen Capture Server started in stdio mode");
    
    var builder = Host.CreateApplicationBuilder();
    builder.Services
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly(typeof(ScreenCaptureTools).Assembly);
    
    var host = builder.Build();
    host.Run();
}, desktopOption);

await rootCmd.InvokeAsync(args);
