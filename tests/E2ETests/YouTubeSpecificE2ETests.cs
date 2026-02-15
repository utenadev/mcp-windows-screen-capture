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
    public async Task WatchVideo_YouTubeGame_AutoPlayAndCapture()
    {
        // Skip on CI
        if (IsCiEnvironment())
            Assert.Ignore("Skipping on CI: Requires YouTube window with game content");

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
        
        // 2. YouTubeウィンドウを探す（"YouTube"を含むタイトル）
        var youtubeWindow = windows.FirstOrDefault(w => 
            w.Title.Contains("YouTube", StringComparison.OrdinalIgnoreCase) ||
            w.Title.Contains("youtube", StringComparison.OrdinalIgnoreCase));
        
        if (youtubeWindow == null)
        {
            Assert.Warn("YouTube window not found. Please open YouTube and play a video before running this test.");
            return;
        }

        Console.WriteLine($"[Test] Found YouTube window: hwnd={youtubeWindow.Hwnd}, title={youtubeWindow.Title}");

        // 3. 30秒待機（ユーザーが動画を準備する時間）
        Console.WriteLine("\n[INFO] 30秒間待機します。動画を再生状態にしてください...");
        Console.WriteLine("（テストは自動的に続行されます）\n");
        
        for (int i = 30; i > 0; i--)
        {
            Console.Write($"\r残り {i} 秒... ");
            await Task.Delay(1000).ConfigureAwait(false);
        }
        
        Console.WriteLine("\n[30秒の待機が完了しました]");

        var hwndStr = youtubeWindow.Hwnd.ToString();

        // 4. ウィンドウ中央をクリックして再生を試みる（念のため）
        Console.WriteLine("\n[INFO] ウィンドウ中央をクリックして再生状態を確認します...");
        int clickX = youtubeWindow.X + (youtubeWindow.W / 2);
        int clickY = youtubeWindow.Y + (youtubeWindow.H / 2);
        
        await _client!.CallToolAsync("input_mouse", new Dictionary<string, object?>
        {
            ["action"] = "click",
            ["x"] = clickX,
            ["y"] = clickY,
            ["button"] = "left",
            ["clicks"] = 1
        }).ConfigureAwait(false);

        Console.WriteLine("[INFO] クリック完了。5秒待機します...");
        await Task.Delay(5000).ConfigureAwait(false);

        // 5. 動画キャプチャを開始
        Console.WriteLine("\n[INFO] visual_watch でキャプチャを開始します...");
        var result = await _client!.CallToolAsync("visual_watch", new Dictionary<string, object?>
        {
            ["mode"] = "video",
            ["target"] = "window",
            ["hwnd"] = hwndStr,
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

        // 6. 3秒間キャプチャを実行
        await Task.Delay(3000).ConfigureAwait(false);

        // 7. ストリームを停止
        var stopResult = await _client.CallToolAsync("visual_stop", new Dictionary<string, object?>
        {
            ["sessionId"] = sessionId
        }).ConfigureAwait(false);

        Assert.That(stopResult, Is.Not.Null);
        Console.WriteLine($"[Test] Video session stopped successfully");
        
        // 最終結果を表示
        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("【テスト結果】");
        Console.WriteLine("YouTubeウィンドウ: 検出OK");
        Console.WriteLine("キャプチャ: OK");
        Console.WriteLine("注意: 動画が再生されているかは手動で確認してください");
        Console.WriteLine(new string('=', 60) + "\n");
    }
}
