using System.CommandLine;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    // Disable logging to stdout for MCP stdio protocol compliance
    builder.Logging.ClearProviders();
    builder.Logging.AddProvider(new StderrLoggerProvider());
    builder.Services
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly(typeof(ScreenCaptureTools).Assembly);
    
    var host = builder.Build();
    host.Run();
}, desktopOption);

await rootCmd.InvokeAsync(args);

// Custom logger that writes to stderr to avoid polluting stdout (MCP stdio protocol)
public class StderrLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new StderrLogger(categoryName);
    public void Dispose() { }
}

public class StderrLogger : ILogger
{
    private readonly string _category;
    public StderrLogger(string category) => _category = category;
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        var message = formatter(state, exception);
        Console.Error.WriteLine($"[{logLevel}] {_category}: {message}");
    }
}

public class NullScope : IDisposable
{
    public static NullScope Instance { get; } = new NullScope();
    public void Dispose() { }
}
