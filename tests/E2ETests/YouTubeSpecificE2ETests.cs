using System.Diagnostics;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using static E2ETests.TestHelper;

namespace E2ETests;

[TestFixture]
public class YouTubeSpecificE2ETests
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
        var testAssemblyDir = Path.GetDirectoryName(typeof(YouTubeSpecificE2ETests).Assembly.Location)!;
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
    public async Task WatchVideo_YouTubeMasterKeaton_ReturnsSessionId()
    {
        // Skip on CI
        if (IsCiEnvironment())
            Assert.Ignore("Skipping on CI: Requires YouTube window");

        // 1. ウィンドウ一覧を取得
        var windowsResult = await _client!.CallToolAsync("visual_list", new Dictionary<string, object?> { ["type"] = "window" })
            .ConfigureAwait(false);
        
        var windowsContent = windowsResult.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.That(windowsContent, Is.Not.Null);

        var windowsJson = windowsContent!.Text ?? "{\"items\":[]}";
        var windowsResponse = TestHelper.DeserializeJson<Dictionary<string, object>>(windowsJson);
        var windows = windowsResponse != null && windowsResponse.ContainsKey("items")
            ? TestHelper.DeserializeJson<List<WindowInfo>>(windowsResponse["items"].ToString()!) ?? new List<WindowInfo>()
            : new List<WindowInfo>();
        
        // 2. 「MASTERキートン」を含むYouTubeウィンドウを探す
        var youtubeWindow = windows.FirstOrDefault(w => 
            (w.Title.Contains("YouTube", StringComparison.OrdinalIgnoreCase) ||
             w.Title.Contains("youtube", StringComparison.OrdinalIgnoreCase)) &&
            w.Title.Contains("MASTERキートン", StringComparison.OrdinalIgnoreCase));
        
        if (youtubeWindow == null)
        {
            Assert.Warn("YouTube window with 'MASTERキートン' not found. Please open YouTube and search for 'MASTERキートン' before running this test.");
            return;
        }

        Console.WriteLine($"[Test] Found YouTube window: hwnd={youtubeWindow.Hwnd}, title={youtubeWindow.Title}");

        // 3. 動画キャプチャを開始
        var result = await _client!.CallToolAsync("visual_watch", new Dictionary<string, object?>
        {
            ["mode"] = "video",
            ["target"] = "window",
            ["hwnd"] = youtubeWindow.Hwnd.ToString(),
            ["fps"] = 5
        }).ConfigureAwait(false);

        Assert.That(result, Is.Not.Null);

        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        Assert.That(textContent, Is.Not.Null);

        // visual_watch returns sessionId as plain string (not JSON)
        var sessionId = textContent!.Text!.Trim('"');
        Assert.That(sessionId, Is.Not.Null.And.Not.Empty);
        Assert.That(Guid.TryParse(sessionId, out _), Is.True);

        Console.WriteLine($"[Test] Video session started: {sessionId}");

        // 4. 3秒間キャプチャを実行
        await Task.Delay(3000).ConfigureAwait(false);

        // 5. ストリームを停止 (get_latest_video_frame is not available in unified API)
        var stopResult = await _client.CallToolAsync("visual_stop", new Dictionary<string, object?>
        {
            ["sessionId"] = sessionId
        }).ConfigureAwait(false);

        Assert.That(stopResult, Is.Not.Null);
        Console.WriteLine($"[Test] Video session stopped");
    }
}
