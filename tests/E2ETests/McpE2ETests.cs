using System.Diagnostics;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using WindowsDesktopUse.Core;

namespace E2ETests;

[TestFixture]
public class McpE2ETests
{
    private static string ServerPath => GetServerPath();
    private McpClient? _client;
    private long? _testNotepadHwnd;
    private HashSet<int> _preExistingNotepadPids = new();

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
        var testAssemblyDir = Path.GetDirectoryName(typeof(McpE2ETests).Assembly.Location)!;
        return Path.GetFullPath(Path.Combine(testAssemblyDir, "..", "..", "..", "..", ".."));
    }

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        // Record existing notepad PIDs to avoid closing them later
        _preExistingNotepadPids = Process.GetProcessesByName("notepad").Select(p => p.Id).ToHashSet();

        // Start a fresh Notepad instance
        Process.Start("notepad.exe");

        // Wait for it to initialize and show window
        await Task.Delay(3000).ConfigureAwait(false);

        _client = await TestHelper.CreateStdioClientAsync(ServerPath, Array.Empty<string>()).ConfigureAwait(false);

        // Find the newly opened notepad window and store its HWND
        var notepad = await FindNotepadWithRetry(5).ConfigureAwait(false);
        if (notepad != null)
        {
            _testNotepadHwnd = notepad.Hwnd;
            Console.WriteLine($"[Setup] Captured test Notepad HWND: {_testNotepadHwnd}");
        }
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        if (_client != null)
        {
            // 1. Try closing by HWND using the new tool
            if (_testNotepadHwnd.HasValue)
            {
                try
                {
                    Console.WriteLine($"[Teardown] Closing test Notepad window (HWND: {_testNotepadHwnd})");
                    await _client.CallToolAsync("input_window", new Dictionary<string, object?> { ["hwnd"] = _testNotepadHwnd.Value.ToString(), ["action"] = "close" }).ConfigureAwait(false);
                }
#pragma warning disable CA1031
                catch { }
#pragma warning restore CA1031
            }

            // 2. Kill any other notepad processes that were started during this test
            var currentNotepads = Process.GetProcessesByName("notepad");
            foreach (var p in currentNotepads)
            {
                if (!_preExistingNotepadPids.Contains(p.Id))
                {
                    try
                    {
                        Console.WriteLine($"[Teardown] Killing unexpected Notepad process (PID: {p.Id})");
                        p.Kill(true);
                    }
#pragma warning disable CA1031
                    catch { }
#pragma warning restore CA1031
                }
            }

            await _client.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task<WindowInfo?> FindNotepadWithRetry(int retryCount = 3)
    {
        for (int i = 0; i < retryCount; i++)
        {
            var windowsResult = await _client!.CallToolAsync("visual_list", new Dictionary<string, object?> { ["type"] = "window" }).ConfigureAwait(false);
            var textContent = windowsResult.Content.OfType<TextContentBlock>().FirstOrDefault();
            if (textContent != null && !string.IsNullOrEmpty(textContent.Text))
            {
                var response = TestHelper.DeserializeJson<Dictionary<string, object>>(textContent.Text);
                if (response != null && response.ContainsKey("items"))
                {
                    var itemsJson = response["items"].ToString();
                    var windows = TestHelper.DeserializeJson<List<WindowInfo>>(itemsJson);
                    if (windows != null)
                    {
                        var notepad = windows.FirstOrDefault(w =>
                            !string.IsNullOrEmpty(w.Title) && (
                            w.Title.Contains("Notepad", StringComparison.OrdinalIgnoreCase) ||
                            w.Title.Contains("メモ帳", StringComparison.Ordinal) ||
                            w.Title.Contains("無題", StringComparison.Ordinal) ||
                            w.Title.Contains("Untitled", StringComparison.OrdinalIgnoreCase)));

                        if (notepad != null) return notepad;
                    }
                }
            }
            await Task.Delay(1000).ConfigureAwait(false);
        }
        return null;
    }

    [Test]
    public async Task E2EListMonitorsReturnsValidMonitors()
    {
        var result = await _client!.CallToolAsync("visual_list", new Dictionary<string, object?> { ["type"] = "monitor" }).ConfigureAwait(false);
        Assert.That(result, Is.Not.Null);

        var textContent = result.Content.OfType<TextContentBlock>().First();
        Console.WriteLine($"[Test] Monitors JSON: {textContent.Text}");
        
        var response = TestHelper.DeserializeJson<Dictionary<string, object>>(textContent.Text);
        Assert.That(response, Is.Not.Null);
        Assert.That(response.ContainsKey("count"), Is.True);
        Assert.That(response.ContainsKey("items"), Is.True);
    }

    [Test]
    public async Task E2EListWindowsReturnsValidWindows()
    {
        var result = await _client!.CallToolAsync("visual_list", new Dictionary<string, object?> { ["type"] = "window" }).ConfigureAwait(false);
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task CameraCaptureStream_WorksCorrectly()
    {
        var monitorsResult = await _client!.CallToolAsync("visual_list", new Dictionary<string, object?> { ["type"] = "monitor" }).ConfigureAwait(false);
        var textContent = monitorsResult.Content.OfType<TextContentBlock>().First();
        var response = TestHelper.DeserializeJson<Dictionary<string, object>>(textContent.Text);
        Assert.That(response, Is.Not.Null);
        Assert.That(response.ContainsKey("items"), Is.True);

        var monitors = TestHelper.DeserializeJson<List<MonitorInfo>>(response["items"].ToString()!);
        Assert.That(monitors, Is.Not.Null);
        Assert.That(monitors.Count, Is.GreaterThan(0));

        var monitor = monitors[0];
        var startX = monitor.X;
        var startY = monitor.Y;
        var width = Math.Min(monitor.W, 640);
        var height = Math.Min(monitor.H, 360);

        Console.WriteLine($"[Test] Starting visual_capture (region): x={startX}, y={startY}, w={width}, h={height}");

        var result = await _client!.CallToolAsync("visual_capture",
            new Dictionary<string, object?>
            {
                ["target"] = "region",
                ["x"] = startX,
                ["y"] = startY,
                ["w"] = width,
                ["h"] = height,
                ["mode"] = "normal"
            }).ConfigureAwait(false);

        Console.WriteLine($"[Test] visual_capture result: {result.Content}");

        Assert.That(result, Is.Not.Null);

        var resultText = result.Content.OfType<TextContentBlock>().First()?.Text;
        Assert.That(resultText, Is.Not.Null);
        Console.WriteLine($"[Test] resultText: {resultText}");

        Assert.That(resultText, Does.Contain("image"));
    }

    [Test]
    public async Task Watch_Tool_WorksCorrectly()
    {
        var result = await _client!.CallToolAsync("visual_watch",
            new Dictionary<string, object?>
            {
                ["mode"] = "unified",
                ["target"] = "monitor",
                ["monitorIndex"] = 0,
                ["fps"] = 1
            }).ConfigureAwait(false);

        Assert.That(result, Is.Not.Null);

        var resultText = result.Content.OfType<TextContentBlock>().First()?.Text;
        Assert.That(resultText, Is.Not.Null);
        
        // visual_watch returns sessionId as plain string (not JSON)
        var sessionId = resultText.Trim('"');
        Assert.That(string.IsNullOrEmpty(sessionId), Is.False);
        Assert.That(Guid.TryParse(sessionId, out _), Is.True);

        await Task.Delay(500).ConfigureAwait(false);

        var stopResult = await _client!.CallToolAsync("visual_stop",
            new Dictionary<string, object?> { ["sessionId"] = sessionId }).ConfigureAwait(false);
        Assert.That(stopResult, Is.Not.Null);
    }

    // ============ PRACTICAL NOTEPAD TESTS ============

    [Test]
    [Order(1)]
    public async Task Notepad1NavigationKeys()
    {
        await _client!.CallToolAsync("keyboard_key", new Dictionary<string, object?> { ["key"] = "enter", ["action"] = "click" }).ConfigureAwait(false);
        await _client.CallToolAsync("keyboard_key", new Dictionary<string, object?> { ["key"] = "space", ["action"] = "click" }).ConfigureAwait(false);
        await Task.Delay(500).ConfigureAwait(false);
    }

    [Test]
    [Order(2)]
    public async Task Notepad2MouseOperations()
    {
        WindowInfo? notepad = null;
        if (_testNotepadHwnd.HasValue)
        {
            var windowsResult = await _client!.CallToolAsync("visual_list", new Dictionary<string, object?> { ["type"] = "window" }).ConfigureAwait(false);
            var textContent = windowsResult.Content.OfType<TextContentBlock>().First();
            var windowsResponse = TestHelper.DeserializeJson<Dictionary<string, object>>(textContent.Text);
            var windows = windowsResponse != null && windowsResponse.ContainsKey("items")
                ? TestHelper.DeserializeJson<List<WindowInfo>>(windowsResponse["items"].ToString()!)
                : null;
            notepad = windows?.FirstOrDefault(w => w.Hwnd == _testNotepadHwnd.Value);
        }

        if (notepad == null)
        {
            Assert.Ignore("Skipping mouse test: Test Notepad window not found in list");
            return;
        }

        int clickX = notepad.X + 50;
        int clickY = notepad.Y + 100;

        await _client!.CallToolAsync("input_mouse", new Dictionary<string, object?> { ["action"] = "move", ["x"] = clickX, ["y"] = clickY }).ConfigureAwait(false);
        await _client!.CallToolAsync("input_mouse", new Dictionary<string, object?> { ["action"] = "click", ["x"] = clickX, ["y"] = clickY, ["button"] = "left", ["clicks"] = 1 }).ConfigureAwait(false);
        await Task.Delay(500).ConfigureAwait(false);

        await _client!.CallToolAsync("input_mouse", new Dictionary<string, object?> { ["action"] = "click", ["x"] = clickX, ["y"] = clickY, ["button"] = "right", ["clicks"] = 1 }).ConfigureAwait(false);
        await Task.Delay(1000).ConfigureAwait(false);

        await _client!.CallToolAsync("keyboard_key", new Dictionary<string, object?> { ["key"] = "escape", ["action"] = "click" }).ConfigureAwait(false);
    }
}
