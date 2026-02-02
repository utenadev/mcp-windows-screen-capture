using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

var client = new HttpClient();
var sseUrl = "http://127.0.0.1:5001/sse";
var messageUrl = "http://127.0.0.1:5001/message";

Console.WriteLine("[Client] Connecting to /sse...");
using var sseResp = await client.GetAsync(sseUrl);
if (!sseResp.IsSuccessStatusCode) {
    Console.Error.WriteLine($"SSE connect failed: {sseResp.StatusCode}");
    return;
}

// Read first event (endpoint)
await using var stream = await sseResp.Content.ReadAsStreamAsync();
var reader = new StreamReader(stream);
string? clientId = null;
while (true) {
    var line = await reader.ReadLineAsync();
    if (line?.StartsWith("event: endpoint") == true) {
        var dataLine = await reader.ReadLineAsync();
        if (dataLine?.StartsWith("data: ") == true) {
            var json = dataLine["data: ".Length..];
            var obj = JsonNode.Parse(json);
            var uri = obj?["uri"]?.GetValue<string>();
            if (uri?.Contains("clientId=") == true) {
                clientId = uri.Split('=')[1];
                Console.WriteLine($"[Client] Got clientId: {clientId}");
                break;
            }
        }
    }
}

if (clientId is null) {
    Console.Error.WriteLine("Failed to get clientId");
    return;
}

// Send capture_screen
var reqJson = new
{
    method = "capture_screen",
    @params = new { monitor = 0 },
    id = 1L
};
var content = new StringContent(JsonSerializer.Serialize(reqJson), System.Text.Encoding.UTF8, "application/json");
var msgResp = await client.PostAsync($"{messageUrl}?clientId={clientId}", content);

Console.WriteLine($"[Client] POST /message status: {msgResp.StatusCode}");
if (msgResp.IsSuccessStatusCode) {
    Console.WriteLine(await msgResp.Content.ReadAsStringAsync());
} else {
    Console.WriteLine("Error response:");
    Console.WriteLine(await msgResp.Content.ReadAsStringAsync());
}