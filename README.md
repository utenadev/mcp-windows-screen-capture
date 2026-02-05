# MCP Windows Screen Capture Server

Windows 11 screen capture MCP server with stdio transport for Claude Desktop.

## About

A Windows screen capture server for AI assistants using the Model Context Protocol (MCP).

## Requirements

- Windows 11 (or Windows 10 1809+)
- Pre-built binaries available in [Releases](../../releases)
- .NET 8.0 SDK (only required for building from source)

## Available MCP Tools

### Screen Capture Tools

| Tool | Description |
|------|-------------|
| `list_monitors` | List all available monitors/displays |
| `see` | Capture a screenshot of specified monitor (like taking a photo with your eyes) |
| `start_watching` | Start a continuous screen capture stream (like watching a live video) |
| `stop_watching` | Stop a running screen capture stream by session ID |
| `get_latest_frame` | Get the latest captured frame with hash for change detection |

### Window Capture Tools

| Tool | Description |
|------|-------------|
| `list_windows` | List all visible Windows applications (hwnd, title, position, size) |
| `capture_window` | Capture a specific window by its HWND (Window handle) |
| `capture_region` | Capture an arbitrary screen region (x, y, width, height) |

## Tool Examples

Ask Claude:
- "See what's on my screen"
- "Look at monitor 1"
- "List all open windows"
- "Capture Visual Studio window"
- "Capture a region from (100,100) to (500,500)"
- "Start watching my screen and tell me when something changes"

### Tool Parameter Examples

```json
// List windows
{"method": "list_windows"}

// Capture specific window
{"method": "capture_window", "arguments": {"hwnd": 123456, "quality": 80}}

// Capture region
{"method": "capture_region", "arguments": {"x": 100, "y": 100, "w": 800, "h": 600}}

// Start watching with HTTP streaming enabled
{"method": "start_watching", "arguments": {"targetType": "monitor", "monitor": 0, "intervalMs": 1000}}
```

## Streaming Features

### Frame Streaming with HTTP Server

The server includes an optional HTTP server that runs in the **same process** as the MCP server for frame streaming. This enables browser-based viewing alongside Claude Desktop interactions.

**Key Points:**
- HTTP server runs in the **same process** as the MCP stdio server (multi-threaded, not multi-process)
- Runs on localhost only for security
- Configure via command-line arguments in your MCP client settings

**Configuration Examples:**

```json
// Default HTTP port (5000)
{
  "mcpServers": {
    "windows-capture": {
      "command": "C:\\path\\to\\WindowsScreenCaptureServer.exe"
    }
  }
}

// Custom HTTP port
{
  "mcpServers": {
    "windows-capture": {
      "command": "C:\\path\\to\\WindowsScreenCaptureServer.exe",
      "args": ["--httpPort", "8080"]
    }
  }
}

// Disable HTTP server (stdio only)
{
  "mcpServers": {
    "windows-capture": {
      "command": "C:\\path\\to\\WindowsScreenCaptureServer.exe",
      "args": ["--httpPort", "0"]
    }
  }
}
```

**HTTP Endpoints:**

| Endpoint | Description |
|----------|-------------|
| `GET /frame/{sessionId}` | Get latest frame as JPEG image |
| `GET /frame/{sessionId}/info` | Get frame metadata (hash, timestamp) |
| `GET /health` | Health check |
| `GET /` | Server info and usage |

**Browser Usage Example:**

```html
<!-- Simple auto-refresh image -->
<img src="http://localhost:5000/frame/SESSION_ID" 
     style="max-width: 100%;" 
     onload="setTimeout(() => this.src = this.src.split('?')[0] + '?' + Date.now(), 1000)">
```

### Frame Polling with MCP Tools

For programmatic access without HTTP server:

```bash
# Start watching
start_watching(targetType="monitor", monitor=0, intervalMs=1000)

# Poll for latest frame
get_latest_frame(sessionId="SESSION_ID")
# Returns: { sessionId, hasFrame, image, hash, captureTime, targetType }

# Check if frame changed by comparing hash values
```

**Change Detection:** The `get_latest_frame` tool returns a SHA256 hash of the frame. Compare this hash with the previous value to detect changes without re-downloading the image data.

## Claude Desktop Configuration

### 1. Download or Build

**Option A: Use Pre-built Release**
- Download the latest release from [Releases](../../releases)
- Extract to your preferred location

**Option B: Build from Source**
```bash
dotnet build src/WindowsScreenCaptureServer.csproj -c Release
```

### 2. Open Claude Desktop settings

1. Launch Claude Desktop
2. Click on Settings (gear icon)
3. Navigate to "MCP Servers"
4. Click "Add MCP Server"

### 3. Add Windows Screen Capture server

Configure MCP server with executable path:

```json
{
  "mcpServers": {
    "windows-capture": {
      "command": "C:\\path\\to\\WindowsScreenCaptureServer.exe"
    }
  }
}
```

Replace the path with the actual location of `WindowsScreenCaptureServer.exe` file on your system.

**With HTTP streaming enabled (optional):**
```json
{
  "mcpServers": {
    "windows-capture": {
      "command": "C:\\path\\to\\WindowsScreenCaptureServer.exe",
      "args": ["--httpPort", "5000"]
    }
  }
}
```

### 4. Save and restart

Click "Save" and restart Claude Desktop.

## Usage Examples

### Screen capture

```
"What's on my screen?"
"Take a screenshot of monitor 0"
```

### Window capture

```
"List all open windows"
"Capture Visual Studio window"
"Show me all visible windows"
```

### Continuous monitoring

```
"Start watching my screen and tell me when something changes"
"Monitor screen for changes"
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| stdio mode not working | Ensure executable path is correct in your MCP client config |
| Server not found | Verify path to `WindowsScreenCaptureServer.exe` exists |
| Black screen | Run with Administrator privileges |
| Window not found | Verify window is visible (not minimized to tray) |
| Claude Desktop can't connect | Check Claude Desktop logs (Settings > Developer > Open Logs) |
| HTTP server not accessible | Ensure port is not blocked by firewall; default is localhost-only |

## License

MIT License - See LICENSE file for details.

---

## Development

> **⚠️ Implementation Note:** This is **GDI+ version** which works reliably without Direct3D dependencies. If you need high-performance GPU capture, you must complete Direct3D/Windows Graphics Capture implementation yourself. This GDI+ version is sufficient for most AI assistant use cases.

### Build & Run

```bash
# Build
dotnet build src/WindowsScreenCaptureServer.csproj -c Release

# The executable is at: src/bin/Release/net8.0-windows/win-x64/WindowsScreenCaptureServer.exe
```

### Testing (stdio mode)

With .NET 8 SDK installed, you can test MCP server directly:

```bash
# Test initialization
echo '{"jsonrpc":"2.0","method":"initialize","params":{"protocolVersion":"2024-11-05"},"id":1}' | src/bin/Release/net8.0-windows/win-x64/WindowsScreenCaptureServer.exe

# Test list_windows tool
echo '{"jsonrpc":"2.0","method":"tools/call","params":{"name":"list_windows","arguments":{}},"id":2}' | src/bin/Release/net8.0-windows/win-x64/WindowsScreenCaptureServer.exe

# Test list_monitors tool
echo '{"jsonrpc":"2.0","method":"tools/call","params":{"name":"list_monitors","arguments":{}},"id":3}' | src/bin/Release/net8.0-windows/win-x64/WindowsScreenCaptureServer.exe
```

The server outputs JSON-RPC responses to stdout and logs to stderr, making it easy to test and debug.

### Limitations & Considerations

#### Window Capture Limitations
- **Minimized Windows**: Windows that are minimized may not be captured correctly or may show stale content. Ensure target window is visible before capturing.
- **GPU-Accelerated Apps**: Uses PW_RENDERFULLCONTENT flag (Windows 8.1+) to capture Chrome, Electron, WPF apps. This works well for static screenshots but may have limitations with some applications.

#### Performance Considerations
- **Static Screenshots**: ✅ Fully supported - capture single screenshots or periodic captures (every few seconds)
- **High-Frequency Video Capture**: ⚠️ Not recommended - CPU load is high. For video/streaming use cases, consider Desktop Duplication API (DirectX-based) instead.
- **Optimal Use Case**: Periodic monitoring, documentation screenshots, automated testing

### Architecture & Implementation

#### Refactoring History

| Version | Changes | Status |
|---------|---------|--------|
| v1.0 | Initial SSE-only implementation | ✅ Merged |
| v1.1 | Tool naming (verbs), inputSchema, error handling | ✅ Merged |
| v1.2 | Unit tests, CI improvements | ✅ Merged |
| v1.3 | Graceful shutdown (IHostApplicationLifetime) | ✅ Merged |
| v1.4 | **Dual Transport** (Streamable HTTP + SSE) | ✅ Merged |
| v1.5 | **Window Capture** (list_windows, capture_window, capture_region) | ✅ Merged |
| v2.0 | **MCP SDK Migration** (Microsoft.ModelContextProtocol) | ✅ Merged |
| v2.1 | **Stdio-only mode** with optional HTTP streaming | ✅ Merged |

#### Key Features

- **Official MCP SDK**: Uses Microsoft.ModelContextProtocol for protocol compliance
- **stdio Transport**: Default local-only mode - secure, no network exposure
- **Optional HTTP Server**: Frame streaming endpoint (same process, localhost only)
- **Window Enumeration**: EnumWindows API for listing visible applications
- **Region Capture**: Arbitrary screen region capture using CopyFromScreen
- **Graceful Shutdown**: Proper cleanup on Ctrl+C or process termination
- **Error Handling**: Comprehensive try-catch blocks with meaningful error messages
- **CI/CD**: GitHub Actions with automated E2E testing
