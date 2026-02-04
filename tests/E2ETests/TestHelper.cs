using System.ComponentModel;
using ModelContextProtocol.Client;

public record WindowInfo(long Hwnd, string Title, int W, int H, int X, int Y);

public record MonitorInfo(uint Idx, string Name, int W, int H, int X, int Y);

public static class TestHelper
{
    public static async Task<McpClient> CreateStdioClientAsync(string serverPath, string[] args)
    {
        var serverTransport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "TestClient",
            Command = serverPath,
            Arguments = args
        });

        var client = await McpClient.CreateAsync(serverTransport);

        return client;
    }

    public static void ValidateBase64Image(string data, int minLength = 1000)
    {
        if (string.IsNullOrEmpty(data))
            throw new ArgumentException("Image data is null or empty");

        if (!data.StartsWith("data:image/") && !data.Contains(";base64,"))
            throw new ArgumentException("Invalid image data format");

        var base64Part = data.Contains(";base64,") 
            ? data.Split(';')[1].Split(',')[1] 
            : data;

        var imageBytes = Convert.FromBase64String(base64Part);
        if (imageBytes.Length < minLength)
            throw new ArgumentException($"Image data too short: {imageBytes.Length} bytes (expected at least {minLength})");
    }
}
