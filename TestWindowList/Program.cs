using System.Diagnostics;
using System.Text.Json;

namespace TestWindowList;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: TestWindowList.exe <path_to_windowsdesktopuse.exe>");
            return;
        }

        var exePath = args[0];
        if (!File.Exists(exePath))
        {
            Console.WriteLine($"Error: File not found: {exePath}");
            return;
        }

        Console.WriteLine($"Testing: {exePath}");
        Console.WriteLine(new string('-', 50));

        try
        {
            var result = await TestMcpServerAsync(exePath);
            Console.WriteLine($"\nResult: {(result ? "SUCCESS ✓" : "FAILED ✗")}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
            Console.WriteLine($"Result: FAILED ✗");
        }
    }

    static async Task<bool> TestMcpServerAsync(string exePath)
    {
        using var process = new Process();
        process.StartInfo.FileName = exePath;
        process.StartInfo.Arguments = "--httpPort 0";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        var responseReceived = false;
        var hasWindows = false;

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine($"[OUT] {e.Data.Substring(0, Math.Min(e.Data.Length, 200))}");
                
                try
                {
                    var json = JsonDocument.Parse(e.Data);
                    if (json.RootElement.TryGetProperty("result", out var result))
                    {
                        responseReceived = true;
                        if (result.TryGetProperty("content", out var content))
                        {
                            foreach (var item in content.EnumerateArray())
                            {
                                if (item.TryGetProperty("text", out var text))
                                {
                                    var textValue = text.GetString() ?? "";
                                    if (textValue.Contains("hwnd") || textValue.Contains("title"))
                                    {
                                        hasWindows = true;
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine($"[ERR] {e.Data.Substring(0, Math.Min(e.Data.Length, 150))}");
            }
        };

        Console.WriteLine("Starting...");
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Initialize
        var initRequest = new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "initialize",
            @params = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new { },
                clientInfo = new { name = "test", version = "1.0" }
            }
        };

        await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(initRequest));
        await Task.Delay(500);

        // List windows
        var listRequest = new
        {
            jsonrpc = "2.0",
            id = 2,
            method = "tools/call",
            @params = new { name = "list_windows", arguments = new { } }
        };

        await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(listRequest));
        await Task.Delay(2000);

        try { process.Kill(); } catch { }

        return responseReceived && hasWindows;
    }
}
