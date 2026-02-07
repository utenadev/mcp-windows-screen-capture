# MCP Windows Screen Capture Server

Windows 11 screen capture MCP server with stdio transport for Claude Desktop.

## Features

- **Screen Capture**: Capture monitors, windows, or regions.
- **Audio Capture**: Record system audio or microphone.
- **Speech Recognition**: Local high-quality transcription using Whisper AI.
- **HTTP Streaming**: View live frames in your browser while interacting with Claude.

## Quick Start

### 1. Build
```bash
dotnet build src/WindowsScreenCaptureServer.csproj -c Release
```

### 2. Configure Claude Desktop
Add this to your `mcpConfig.json`:
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

## Available MCP Tools (Summary)

| Tool | Description |
|------|-------------|
| `list_all` | List all monitors and windows |
| `capture` | Capture any target as image |
| `watch` | Start streaming a target |
| `listen` | Transcribe audio to text |

For a full list of tools and detailed usage, see [**Tools Guide**](docs/TOOLS.md).

## Documentation Index

- [**Tools Reference**](docs/TOOLS.md) - Detailed command list and examples.
- [**Development Guide**](docs/DEVELOPMENT.md) - Build, test, and architecture details.
- [**Whisper AI**](docs/WHISPER.md) - Speech recognition features and models.

## Requirements

- Windows 11 (or Windows 10 1803+)
- .NET 8.0 Runtime/SDK

## License
MIT License. See [LICENSE](LICENSE) file.