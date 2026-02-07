# Development Guide

This document contains information for developers who want to build, test, or contribute to the MCP Windows Screen Capture Server.

## Architecture

The server is built with .NET 8 and uses the following architectural pattern:

- **Program.cs**: Entry point. Handles command-line arguments, initializes services, and starts the MCP host.
- **Services/**: Core logic for specific domains.
  - `ScreenCaptureService`: Handles GDI+ screen/window capture and session management.
  - `AudioCaptureService`: Handles NAudio-based audio recording.
  - `WhisperTranscriptionService`: Integrates with Whisper.net for AI transcription.
- **Tools/ScreenCaptureTools.cs**: Defines the MCP tools interface. It maps MCP tool calls to the underlying services.
- **CaptureServices/**: Infrastructure for modern capture APIs (WIP).

### Important: Stdio Protocol & Logging
When adding logs, **always use `Console.Error.WriteLine`**. Writing to `stdout` will break the JSON-RPC communication with the MCP client (like Claude Desktop).

---

## Building

### Requirements
- Windows 11 (or 10 1803+)
- .NET 8.0 SDK

### Build Commands
```bash
# Build the project
dotnet build src/WindowsScreenCaptureServer.csproj -c Release

# The executable will be at:
# src/bin/Release/net8.0-windows/win-x64/WindowsScreenCaptureServer.exe
```

---

## Testing

### E2E Tests
The project includes E2E tests that simulate MCP interactions using `stdio`.
```bash
dotnet test tests/E2ETests/E2ETests.csproj
```

### Manual Testing (stdio)
You can test tool calls directly via pipe:
```bash
# Test initialization
echo '{"jsonrpc":"2.0","method":"initialize","params":{"protocolVersion":"2024-11-05"},"id":1}' | path/to/WindowsScreenCaptureServer.exe
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Black screen | Ensure the app is not running in a restricted session. Try running as Administrator. |
| High CPU | Streaming/Watching tools are CPU-intensive due to GDI+ capture. Increase `intervalMs` or stop inactive sessions. |
| Model download fails | Ensure internet access to Hugging Face. Models are stored in `models/` subdirectory of the executable. |
