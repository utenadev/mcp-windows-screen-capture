# GEMINI.md - Instructional Context for MCP Windows Screen Capture Server

This document provides essential context and instructions for AI agents working on the **MCP Windows Screen Capture Server** project.

## Project Overview

The **MCP Windows Screen Capture Server** is a Model Context Protocol (MCP) server implementation for Windows 11. It enables AI assistants (like Claude Desktop) to "see" and "hear" the Windows environment through a standardized interface.

### Main Technologies
- **Runtime:** .NET 8.0 (Windows-specific: `net8.0-windows`)
- **MCP SDK:** `Microsoft.ModelContextProtocol`
- **Screen Capture:** GDI+ (`System.Drawing.Common`) and Win32 APIs. Foundation for `Windows.Graphics.Capture` is present.
- **Audio Capture:** `NAudio` (WASAPI for system audio, WaveIn for microphones).
- **Speech Recognition:** `Whisper.net` (OpenAI Whisper bindings for .NET).
- **CLI Framework:** `System.CommandLine`

### Key Features
- **Screen Capture:** Monitors, specific windows (by HWND), and arbitrary regions.
- **Unified Tools:** Simplified tools like `list_all`, `capture`, and `watch`.
- **Audio Capture:** System audio and microphone recording.
- **Speech-to-Text:** Real-time or file-based transcription using Whisper AI.
- **Dual Transport:** Standard MCP `stdio` transport with an optional `HTTP` server for frame streaming.

## Building and Running

### Prerequisites
- Windows 11 (or Windows 10 1803+)
- .NET 8.0 SDK

### Key Commands
| Action | Command |
|--------|---------|
| **Build (Release)** | `dotnet build src/WindowsScreenCaptureServer.csproj -c Release` |
| **Run (stdio mode)** | `src/bin/Release/net8.0-windows/win-x64/WindowsScreenCaptureServer.exe` |
| **Run (HTTP mode)** | `src/bin/Release/net8.0-windows/win-x64/WindowsScreenCaptureServer.exe --httpPort 5000` |
| **Test Whisper** | `src/bin/Release/net8.0-windows/win-x64/WindowsScreenCaptureServer.exe --test-whisper` |
| **Run E2E Tests** | `dotnet test tests/E2ETests/E2ETests.csproj` |

## Development Conventions

### Coding Style & Architecture
- **Service Pattern:** Core logic is encapsulated in services under `src/Services/` (e.g., `ScreenCaptureService`, `AudioCaptureService`).
- **MCP Tools:** Defined in `src/Tools/ScreenCaptureTools.cs` using `[McpServerTool]` and `[McpServerToolType]` attributes.
- **Asynchronous Code:** Prefer `async/await` for I/O bound operations, though some Win32/GDI+ calls are synchronous.
- **Error Handling:** Use comprehensive `try-catch` blocks in tool implementations to return meaningful error messages to the MCP client.

### Logging
- **STRICT REQUIREMENT:** Always use `Console.Error.WriteLine` for logging.
- **REASON:** The `stdio` transport uses `stdout` for JSON-RPC messages. Writing logs to `stdout` will corrupt the protocol stream and cause the MCP client to disconnect.

### Testing Practices
- **E2E Tests:** Use `NUnit` and a helper `StdioClient` to simulate actual MCP interactions.
- **Platform Specificity:** Tests often require an active Windows session and may be sensitive to DPI settings or window visibility.
- **Environment Variables:** `GITHUB_WORKSPACE` is used in CI; `KEEP_AUDIO_FILE=true` can be used to preserve test recordings.

## Project Structure

- `src/`: Primary source code.
    - `Program.cs`: Entry point, argument parsing, HTTP server setup.
    - `Services/`: Business logic for capture and transcription.
    - `Tools/`: MCP tool definitions.
    - `CaptureServices/`: Foundation for modern capture APIs.
- `tests/`: Integration and E2E tests.
- `docs/`: Feature proposals, unification guides, and roadmaps.
- `publish/`: Target directory for deployment builds.

## Important Notes for AI Agents
- **Win32 Interaction:** Be careful with DPI awareness (`SetProcessDPIAware`) when calculating screen coordinates.
- **Whisper Models:** Models are automatically downloaded from Hugging Face on first use.
- **GDI+ vs Modern Capture:** The current implementation primarily uses GDI+ for compatibility. `ModernCaptureService` is a stub for future `Windows.Graphics.Capture` implementation.
- **Japanese Language:** This project has specific support and documentation for Japanese users (`README.ja.md`). Source comments and commits should be in English.
