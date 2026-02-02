$baseUrl = "http://127.0.0.1:5001"
$client = [System.Net.Http.HttpClient]::new()

try {
    # Step 1: Connect to /sse and get clientId
    $sseResp = $client.GetAsync("$baseUrl/sse").Result
    if (-not $sseResp.IsSuccessStatusCode) {
        Write-Error "SSE connect failed: $($sseResp.StatusCode)"
        return
    }

    $stream = $sseResp.Content.ReadAsStreamAsync().Result
    $reader = [System.IO.StreamReader]::new($stream)
    $clientId = $null

    while ($true) {
        $line = $reader.ReadLine()
        if ($line -and $line.StartsWith("event: endpoint")) {
            $dataLine = $reader.ReadLine()
            if ($dataLine -and $dataLine.StartsWith("data: ")) {
                $json = $dataLine.Substring(6)
                $obj = $json | ConvertFrom-Json
                $uri = $obj.uri
                if ($uri -and $uri.Contains("clientId=")) {
                    $clientId = $uri.Split('=')[1]
                    Write-Host "[Client] Got clientId: $clientId"
                    break
                }
            }
        }
    }

    if (-not $clientId) {
        Write-Error "Failed to get clientId"
        return
    }

    # Step 2: POST capture_screen
    $reqBody = @{
        method = "capture_screen"
        params = @{ monitor = 0 }
        id = 1
    } | ConvertTo-Json
    $content = [System.Net.Http.StringContent]::new($reqBody, [System.Text.Encoding]::UTF8, "application/json")
    $msgResp = $client.PostAsync("$baseUrl/message?clientId=$clientId", $content).Result

    Write-Host "[Client] POST status: $($msgResp.StatusCode)"
    $respBody = $msgResp.Content.ReadAsStringAsync().Result
    Write-Host $respBody

} finally {
    $client.Dispose()
}