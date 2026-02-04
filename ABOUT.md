# About

MCP Windows Screen Capture Server is a Windows 11 screen capture MCP (Model Context Protocol) server designed for AI assistants like Claude.

## Overview

This server provides AI assistants with the ability to:
- Capture screenshots of monitors, windows, and screen regions
- List available monitors and visible windows
- Monitor screen changes over time (continuous capture)
- Support Claude Desktop via stdio transport

## Technology

- **Language**: C# (.NET 8.0)
- **Screen Capture**: GDI+ (Windows Graphics Device Interface)
- **MCP Protocol**: Official Microsoft.ModelContextProtocol SDK
- **Transport**: stdio (for Claude Desktop)

## Version History

| Version | Date | Description |
|---------|------|-------------|
| v2.1.0 | 2026-02-04 | Stdio-only mode, removed HTTP |
| v2.0.0 | 2026-02-04 | MCP SDK migration |
| v1.5.0 | 2026-01-XX | Window capture tools |
| v1.4.0 | 2026-01-XX | Dual transport (HTTP + SSE) |
| v1.3.0 | 2026-01-XX | Graceful shutdown |
| v1.2.0 | 2026-01-XX | Unit tests |
| v1.1.0 | 2026-01-XX | Tool improvements |
| v1.0.0 | 2026-01-XX | Initial release |

## License

MIT License - See [LICENSE](LICENSE) file for details.

## Links

- [GitHub Repository](https://github.com/utenadev/mcp-windows-screen-capture)
- [Releases](https://github.com/utenadev/mcp-windows-screen-capture/releases)
- [Documentation](README.md)
- [Issues](https://github.com/utenadev/mcp-windows-screen-capture/issues)
