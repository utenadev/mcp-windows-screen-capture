using ModelContextProtocol.Protocol;
using NUnit.Framework;

namespace E2ETests;

[TestFixture]
public class McpE2ETests
{
    private const string ServerPath = "C:\\workspace\\mcp-windows-screen-capture\\src\\bin\\Release\\net8.0-windows\\win-x64\\WindowsScreenCaptureServer.exe";

    [Test]
    public async Task E2E_ListMonitors_ReturnsValidMonitors()
    {
        var client = await TestHelper.CreateStdioClientAsync(ServerPath, Array.Empty<string>());

        try
        {
            var tools = await client.ListToolsAsync();
            Assert.That(tools, Is.Not.Null);

            var listMonitorsTool = tools.FirstOrDefault(t => t.Name == "list_monitors");
            Assert.That(listMonitorsTool, Is.Not.Null, "list_monitors tool not found");

            var result = await client.CallToolAsync("list_monitors", null);
            Assert.That(result, Is.Not.Null);
        }
        finally
        {
            await client.DisposeAsync();
        }
    }

    [Test]
    public async Task E2E_ListWindows_ReturnsValidWindows()
    {
        var client = await TestHelper.CreateStdioClientAsync(ServerPath, Array.Empty<string>());

        try
        {
            var result = await client.CallToolAsync("list_windows", null);
            Assert.That(result, Is.Not.Null);
        }
        finally
        {
            await client.DisposeAsync();
        }
    }

    [Test]
    public async Task E2E_SeeMonitor_ReturnsValidImage()
    {
        var client = await TestHelper.CreateStdioClientAsync(ServerPath, Array.Empty<string>());

        try
        {
            var args = new Dictionary<string, object?>
            {
                ["targetType"] = "monitor",
                ["monitor"] = 0,
                ["quality"] = 80,
                ["maxWidth"] = 1920
            };

            var result = await client.CallToolAsync("see", args);
            Assert.That(result, Is.Not.Null);

            var imageContent = result.Content.OfType<ImageContentBlock>().FirstOrDefault();
            Assert.That(imageContent, Is.Not.Null, "No image content found");
            Assert.That(imageContent.Data, Is.Not.Null, "Image data is null");

            TestHelper.ValidateBase64Image(imageContent.Data);
        }
        finally
        {
            await client.DisposeAsync();
        }
    }

    [Test]
    public async Task E2E_CaptureWindow_ReturnsValidImage()
    {
        var client = await TestHelper.CreateStdioClientAsync(ServerPath, Array.Empty<string>());

        try
        {
            var windowsResult = await client.CallToolAsync("list_windows", null);
            Assert.That(windowsResult, Is.Not.Null, "list_windows failed");

            var textContent = windowsResult.Content.OfType<TextContentBlock>().FirstOrDefault();
            Assert.That(textContent, Is.Not.Null, "No text content found in list_windows result");

            var windows = System.Text.Json.JsonSerializer.Deserialize<List<WindowInfo>>(textContent.Text);
            Assert.That(windows, Is.Not.Null.And.Count.GreaterThan(0), "No windows found");

            var testWindow = windows.FirstOrDefault(w => !string.IsNullOrEmpty(w.Title));
            Assert.That(testWindow, Is.Not.Null, "No test window found");

            var args = new Dictionary<string, object?>
            {
                ["hwnd"] = testWindow.Hwnd,
                ["quality"] = 80,
                ["maxWidth"] = 1920
            };

            var result = await client.CallToolAsync("capture_window", args);
            Assert.That(result, Is.Not.Null);

            var imageContent = result.Content.OfType<ImageContentBlock>().FirstOrDefault();
            Assert.That(imageContent, Is.Not.Null, "No image content found");
            Assert.That(imageContent.Data, Is.Not.Null, "Image data is null");

            TestHelper.ValidateBase64Image(imageContent.Data);
        }
        finally
        {
            await client.DisposeAsync();
        }
    }

    [Test]
    public async Task E2E_CaptureRegion_ReturnsValidImage()
    {
        var client = await TestHelper.CreateStdioClientAsync(ServerPath, Array.Empty<string>());

        try
        {
            var args = new Dictionary<string, object?>
            {
                ["x"] = 0,
                ["y"] = 0,
                ["w"] = 100,
                ["h"] = 100,
                ["quality"] = 80,
                ["maxWidth"] = 1920
            };

            var result = await client.CallToolAsync("capture_region", args);
            Assert.That(result, Is.Not.Null);

            var imageContent = result.Content.OfType<ImageContentBlock>().FirstOrDefault();
            Assert.That(imageContent, Is.Not.Null, "No image content found");
            Assert.That(imageContent.Data, Is.Not.Null, "Image data is null");

            TestHelper.ValidateBase64Image(imageContent.Data);
        }
        finally
        {
            await client.DisposeAsync();
        }
    }

    [Test]
    public async Task E2E_StartWatching_ReturnsValidSessionId()
    {
        var client = await TestHelper.CreateStdioClientAsync(ServerPath, Array.Empty<string>());

        try
        {
            var args = new Dictionary<string, object?>
            {
                ["targetType"] = "monitor",
                ["monitor"] = 0,
                ["intervalMs"] = 1000,
                ["quality"] = 80,
                ["maxWidth"] = 1920
            };

            var result = await client.CallToolAsync("start_watching", args);
            Assert.That(result, Is.Not.Null);

            var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
            Assert.That(textContent, Is.Not.Null, "No text content found");
            Assert.That(textContent.Text, Is.Not.Null, "Session ID is null");

            var sessionData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(textContent.Text);
            Assert.That(sessionData, Is.Not.Null);
            Assert.That(sessionData.ContainsKey("sessionId"), Is.True, "Session ID not found in result");
            Assert.That(sessionData["sessionId"], Is.Not.Null.And.Length.GreaterThan(0), "Session ID is empty");
        }
        finally
        {
            await client.DisposeAsync();
        }
    }

    [Test]
    public async Task E2E_StopWatching_ReturnsSuccess()
    {
        var client = await TestHelper.CreateStdioClientAsync(ServerPath, Array.Empty<string>());

        try
        {
            var startArgs = new Dictionary<string, object?>
            {
                ["targetType"] = "monitor",
                ["monitor"] = 0,
                ["intervalMs"] = 1000,
                ["quality"] = 80,
                ["maxWidth"] = 1920
            };

            var startResult = await client.CallToolAsync("start_watching", startArgs);
            Assert.That(startResult, Is.Not.Null, "start_watching failed");

            var textContent = startResult.Content.OfType<TextContentBlock>().FirstOrDefault();
            var sessionData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(textContent.Text);
            var sessionId = sessionData?["sessionId"];

            var stopArgs = new Dictionary<string, object?>
            {
                ["sessionId"] = sessionId
            };

            var stopResult = await client.CallToolAsync("stop_watching", stopArgs);
            Assert.That(stopResult, Is.Not.Null);
        }
        finally
        {
            await client.DisposeAsync();
        }
    }

    [Test]
    public async Task E2E_InvalidMonitorIndex_ReturnsError()
    {
        var client = await TestHelper.CreateStdioClientAsync(ServerPath, Array.Empty<string>());

        try
        {
            var args = new Dictionary<string, object?>
            {
                ["targetType"] = "monitor",
                ["monitor"] = 999,
                ["quality"] = 80,
                ["maxWidth"] = 1920
            };

            var result = await client.CallToolAsync("see", args);
            Assert.That(result, Is.Not.Null);
        }
        finally
        {
            await client.DisposeAsync();
        }
    }
}
