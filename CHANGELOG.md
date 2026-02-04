# Changelog

All notable changes to MCP Windows Screen Capture Server will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.1.0] - 2026-02-04

### Changed
- Simplified server to stdio-only mode (removed HTTP mode)
- Removed ~500 lines of code (StreamableHttpServer.cs, McpSession.cs)
- Updated documentation to focus on Claude Desktop use case
- Improved GitHub Actions workflow (artifacts only on release)

### Removed
- HTTP mode and related CLI options (`--http`, `--ip_addr`, `--port`)
- StreamableHttpServer.cs
- McpSession.cs

### Added
- CONTRIBUTING.md with development guidelines
- CHANGELOG.md for version tracking

## [2.0.0] - 2026-02-04

### Added
- Migrated to official Microsoft.ModelContextProtocol SDK (0.7.0-preview.1)
- E2E test suite with 8 automated tests
- Comprehensive test coverage for all MCP tools
- GitHub Actions workflow for automated testing
- Official MCP protocol compliance
- ImageContentBlock return type for image tools

### Changed
- All MCP tools now return proper MCP protocol content types
- Simplified JSON-RPC handling (SDK-based)
- Updated README.md and README.ja.md for Claude Desktop integration
- Changed default transport to stdio (recommended for Claude Desktop)

### Fixed
- JSON-RPC response format issues
- Zod validation errors in Claude Desktop
- Tool parameter handling

### Removed
- Manual JSON-RPC implementation (replaced by SDK)

## [1.5.0] - 2026-01-XX

### Added
- Window enumeration tool (`list_windows`)
- Window capture tool (`capture_window`)
- Region capture tool (`capture_region`)
- Window capture with PW_RENDERFULLCONTENT flag for GPU-accelerated apps
- Stream sessions for window watching

### Changed
- Improved window capture reliability
- Better error handling for invalid window handles

## [1.4.0] - 2026-01-XX

### Added
- Dual transport support (Streamable HTTP + SSE)
- MCP-Session-Id header for session management
- Session cleanup on connection close
- Stream endpoint (`/stream/{id}`) for continuous capture

### Changed
- Improved HTTP mode stability
- Better error messages for connection issues

## [1.3.0] - 2026-01-XX

### Added
- Graceful shutdown using IHostApplicationLifetime
- Proper cleanup on Ctrl+C or process termination
- Cleanup logging

### Changed
- Improved startup and shutdown experience

## [1.2.0] - 2026-01-XX

### Added
- Unit tests
- CI improvements (GitHub Actions)

### Changed
- Better error handling
- Improved test coverage

## [1.1.0] - 2026-01-XX

### Added
- Tool naming improvements (verbs)
- Input schema for all tools
- Better error handling
- Parameter validation

### Changed
- Improved tool descriptions
- Better error messages

## [1.0.0] - 2026-01-XX

### Added
- Initial SSE-only implementation
- Screen capture tools (`list_monitors`, `see`, `start_watching`, `stop_watching`)
- Basic session management
- GDI+ based screen capture

## [Unreleased] - 2026-01-XX

### Added
- Initial project setup
- Basic MCP server implementation
- Screen capture functionality

[Unreleased]: https://github.com/utenadev/mcp-windows-screen-capture/compare/v2.1.0...HEAD
[2.1.0]: https://github.com/utenadev/mcp-windows-screen-capture/compare/v2.0.0...v2.1.0
[2.0.0]: https://github.com/utenadev/mcp-windows-screen-capture/compare/v1.5.0...v2.0.0
[1.5.0]: https://github.com/utenadev/mcp-windows-screen-capture/compare/v1.4.0...v1.5.0
[1.4.0]: https://github.com/utenadev/mcp-windows-screen-capture/compare/v1.3.0...v1.4.0
[1.3.0]: https://github.com/utenadev/mcp-windows-screen-capture/compare/v1.2.0...v1.3.0
[1.2.0]: https://github.com/utenadev/mcp-windows-screen-capture/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/utenadev/mcp-windows-screen-capture/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/utenadev/mcp-windows-screen-capture/releases/tag/v1.0.0
