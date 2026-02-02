@echo off
setlocal enabledelayedexpansion

echo [TEST] Windows Screen Capture Client (MCP)
echo.

:: Step 1: Get clientId from /sse
echo [1/3] Connecting to /sse...
for /f "tokens=*" %%a in ('curl -s http://127.0.0.1:5001/sse --max-time 3 ^| findstr "clientId"') do (
    set "line=%%a"
)
if "!line!"=="" (
    echo ERROR: Failed to get clientId. Is server running?
    pause
    exit /b 1
)

:: Extract clientId from: data: {"uri":"/message?clientId=xxx"}
echo !line! > temp_line.txt
for /f "tokens=2 delims==" %%i in ('findstr "clientId=" temp_line.txt') do set "CLIENT_ID=%%i"
del temp_line.txt

if "!CLIENT_ID!"=="" (
    echo ERROR: clientId not found in response.
    pause
    exit /b 1
)
echo [OK] clientId = !CLIENT_ID!

:: Step 2: Send capture_screen
echo [2/3] Sending capture_screen...
set "REQ={\"method\":\"capture_screen\",\"params\":{\"monitor\":0},\"id\":1}"
curl -X POST "http://127.0.0.1:5001/message?clientId=!CLIENT_ID!" ^
  -H "Content-Type: application/json" ^
  -d "!REQ!" > response.json

:: Check if response has content
findstr "content" response.json >nul
if errorlevel 1 (
    echo ERROR: Server returned no content (HTTP 500?).
    type response.json
    pause
    exit /b 1
)
echo [OK] Response saved to response.json

:: Step 3: Extract and save image
echo [3/3] Saving screenshot as screen.jpg...
powershell -Command ^
  "$r = Get-Content response.json | ConvertFrom-Json; ^
   $img = $r.result.content | Where-Object { $_.type -eq 'image' } | Select-Object -First 1; ^
   if ($img) { ^
     $b64 = $img.data.Split(',')[1]; ^
     $bytes = [System.Convert]::FromBase64String($b64); ^
     [System.IO.File]::WriteAllBytes('screen.jpg', $bytes); ^
     echo '[OK] screen.jpg saved.'; ^
   } else { ^
     echo '[ERROR] No image found in response.'; ^
   }"

echo.
echo === DONE ===
echo - response.json: raw MCP response
echo - screen.jpg   : captured screenshot (if successful)
pause