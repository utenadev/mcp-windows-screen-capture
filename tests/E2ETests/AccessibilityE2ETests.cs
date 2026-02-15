using System.Diagnostics;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using static E2ETests.TestHelper;

namespace E2ETests;

[TestFixture]
public class AccessibilityE2ETests
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
        var testAssemblyDir = Path.GetDirectoryName(typeof(AccessibilityE2ETests).Assembly.Location)!;
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
    public async Task ReadWindowText_WithNotepad_ReturnsText()
    {
        // Start Notepad
        var notepadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "notepad.exe");
        using var notepadProcess = Process.Start(notepadPath);
        Assert.That(notepadProcess, Is.Not.Null);

        try
        {
            // Wait for Notepad to start
            await Task.Delay(3000).ConfigureAwait(false);

            // Get windows list and find Notepad
            var windowsResult = await _client!.CallToolAsync("visual_list", new Dictionary<string, object?> { ["type"] = "window" })
                .ConfigureAwait(false);
            
            var windowsContent = windowsResult.Content.OfType<TextContentBlock>().FirstOrDefault();
            Assert.That(windowsContent, Is.Not.Null);

            var windowsJson = windowsContent!.Text ?? "{\"items\":[]}";
            var windowsResponse = TestHelper.DeserializeJson<Dictionary<string, object>>(windowsJson);
            var windows = windowsResponse != null && windowsResponse.ContainsKey("items") 
                ? TestHelper.DeserializeJson<List<WindowInfo>>(windowsResponse["items"].ToString()!) ?? new List<WindowInfo>()
                : new List<WindowInfo>();
            
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

            // Call read_window_text
            var result = await _client!.CallToolAsync("read_window_text", new Dictionary<string, object?>
            {
                ["hwndStr"] = hwnd.ToString(),
                ["includeButtons"] = false
            }).ConfigureAwait(false);

            Assert.That(result, Is.Not.Null);

            var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
            Assert.That(textContent, Is.Not.Null);
            Assert.That(textContent!.Text, Is.Not.Null);

            Console.WriteLine($"[Test] ReadWindowText result: {textContent.Text}");

            // Should contain some content
            Assert.That(textContent.Text!.Length, Is.GreaterThan(0));
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
    public async Task ReadWindowText_InvalidHwnd_ReturnsEmptyOrError()
    {
        var result = await _client!.CallToolAsync("read_window_text", new Dictionary<string, object?>
        {
            ["hwndStr"] = "-1", // Invalid handle
            ["includeButtons"] = false
        }).ConfigureAwait(false);

        Assert.That(result, Is.Not.Null);

        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.That(textContent, Is.Not.Null);

        Console.WriteLine($"[Test] ReadWindowText invalid result: {textContent!.Text}");
    }
}
