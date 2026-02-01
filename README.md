# MCP Windows Screen Capture Server

Windows 11 screen capture MCP server with `--ip_addr`, `--port`, `--desktopNum` CLI options.

> **⚠️ Implementation Note:** This is the **GDI+ version** which works reliably without Direct3D dependencies. If you need high-performance GPU capture, you must complete the Direct3D/Windows Graphics Capture implementation yourself. This GDI+ version is sufficient for most AI assistant use cases.

## Requirements
- Windows 11 (or Windows 10 1809+)
- .NET 8.0 SDK

## Build & Run

```bash
# Build
dotnet build -c Release

# Run with CLI options (Required for WSL2)
dotnet run -- --ip_addr 0.0.0.0 --port 5000 --desktopNum 0

# Or single-file publish
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
```

## CLI Options
- `--ip_addr`: IP to bind (`0.0.0.0` for WSL2 access, `127.0.0.1` for local only)
- `--port`: Port number (default: 5000)
- `--desktopNum`: Default monitor index (0=primary, 1=secondary, etc.)

## Claude Code Configuration

### Windows (Native)
`~/.claude/config.json`:
```json
{
  "mcpServers": {
    "windows-capture": {
      "command": "curl",
      "args": ["-N", "http://127.0.0.1:5000/sse"]
    }
  }
}
```

### WSL2
```json
{
  "mcpServers": {
    "windows-capture": {
      "command": "bash",
      "args": [
        "-c",
        "WIN_IP=$(ip route | grep default | awk '{print $3}'); curl -N http://${WIN_IP}:5000/sse"
      ]
    }
  }
}
```

## First Run (Firewall)
Run as Administrator in PowerShell:
```powershell
# For WSL2 subnet only (secure)
netsh advfirewall firewall add rule name="MCP Screen Capture" dir=in action=allow protocol=TCP localport=5000 remoteip=172.16.0.0/12
```
