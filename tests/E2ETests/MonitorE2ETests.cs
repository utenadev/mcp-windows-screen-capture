using System.Diagnostics;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using static E2ETests.TestHelper;

namespace E2ETests;

[TestFixture]
public class MonitorE2ETests
{
    private static string ServerPath => GetServerPath();
    private McpClient? _client;

    private static string GetServerPath()
    {
        var githubWorkspace = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE");
        var repoRoot = !string.IsNullOrEmpty(githubWorkspace)
            ? githubWorkspace
            : GetRepoRootFromAssembly();

        var possiblePaths = new[]
        {
            Path.Combine(repoRoot, "src", "WindowsDesktopUse.App", "bin", "Debug", "net8.0-windows", "win-x64", "WindowsDesktopUse.exe"),
            Path.Combine(repoRoot, "src", "WindowsDesktopUse.App", "bin", "Release", "net8.0-windows", "win-x64", "WindowsDesktopUse.exe"),
            ""
        };

        foreach (var path in possiblePaths)
        {
            if (string.IsNullOrEmpty(path)) continue;
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath)) return fullPath;
        }

        throw new FileNotFoundException($"WindowsDesktopUse.exe not found.");
    }

    private static string GetRepoRootFromAssembly()
    {
        var testAssemblyDir = Path.GetDirectoryName(typeof(MonitorE2ETests).Assembly.Location)!;
        return Path.GetFullPath(Path.Combine(testAssemblyDir, "..", "..", "..", "..", ".."));
    }

    [SetUp]
    public async Task Setup()
    {
        _client = await CreateStdioClientAsync(ServerPath, Array.Empty<string>()).ConfigureAwait(false);
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_client != null)
        {
            await _client.DisposeAsync().ConfigureAwait(false);
        }
    }

    [Test]
    public async Task Monitor_WithNotepad_ReturnsSessionId()
    {
        // Skip on CI
        if (IsCiEnvironment())
            Assert.Ignore("Skipping on CI: Requires desktop window to be visible");

        // Start Notepad
        var notepadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "notepad.exe");
        using var notepadProcess = Process.Start(notepadPath);
        Assert.That(notepadProcess, Is.Not.Null);

        try
        {
            // Wait for Notepad to start
            await Task.Delay(3000).ConfigureAwait(false);

            // Get windows list and find Notepad
            var windowsResult = await _client!.CallToolAsync("list_windows", new Dictionary<string, object?>())
                .ConfigureAwait(false);
            
            var windowsContent = windowsResult.Content.OfType<TextContentBlock>().FirstOrDefault();
            Assert.That(windowsContent, Is.Not.Null);

            var windowsJson = windowsContent!.Text ?? "[]";
            var windows = TestHelper.DeserializeJson<List<WindowInfo>>(windowsJson) ?? new List<WindowInfo>();
            
            // Find Notepad window
            var notepadWindow = windows.FirstOrDefault(w => w.Title.Contains("Notepad", StringComparison.OrdinalIgnoreCase) || 
                                                             w.Title.Contains("メモ帳", StringComparison.OrdinalIgnoreCase));
            
            if (notepadWindow == null)
            {
                Assert.Warn("Notepad window not found in window list");
                return;
            }

            var hwnd = notepadWindow.Hwnd;
            Console.WriteLine($"[Test] Found Notepad window: hwnd={hwnd}, title={notepadWindow.Title}");

            // Start monitoring
            var result = await _client!.CallToolAsync("monitor", new Dictionary<string, object?>
            {
                ["hwnd"] = hwnd,
                ["sensitivity"] = "Medium",
                ["intervalMs"] = 1000
            }).ConfigureAwait(false);

            Assert.That(result, Is.Not.Null);

            var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
            Assert.That(textContent, Is.Not.Null);

            var sessionId = textContent!.Text?.Trim('"');
            Assert.That(sessionId, Is.Not.Null.And.Not.Empty);
            Assert.That(Guid.TryParse(sessionId, out _), Is.True);

            Console.WriteLine($"[Test] Monitor session started: {sessionId}");

            // Let it run for a short time
            await Task.Delay(3000).ConfigureAwait(false);

            // Stop monitoring
            var stopResult = await _client.CallToolAsync("stop_monitor", new Dictionary<string, object?>
            {
                ["sessionId"] = sessionId
            }).ConfigureAwait(false);

            Assert.That(stopResult, Is.Not.Null);
            Console.WriteLine($"[Test] Monitor stopped successfully");
        }
        finally
        {
            if (!notepadProcess.HasExited)
            {
                notepadProcess.Kill();
            }
        }
    }

    [Test]
    public async Task StopMonitor_InvalidSession_ReturnsMessage()
    {
        var result = await _client!.CallToolAsync("stop_monitor", new Dictionary<string, object?>
        {
            ["sessionId"] = "non-existent-session"
        }).ConfigureAwait(false);

        Assert.That(result, Is.Not.Null);

        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.That(textContent, Is.Not.Null);

        Console.WriteLine($"[Test] StopMonitor result: {textContent!.Text}");
    }
}
