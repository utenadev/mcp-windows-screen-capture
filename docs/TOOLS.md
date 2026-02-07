# MCP Windows Screen Capture Tools

This document provides detailed information about the MCP tools available in this server.

## Summary Table

| Category | Tool | Description |
|----------|------|-------------|
| **Screen** | `list_monitors` | List all available monitors |
| | `see` | Capture a screenshot of a monitor or window |
| | `capture_region` | Capture an arbitrary screen region |
| | `start_watching` | Start a continuous screen capture stream |
| | `stop_watching` | Stop a running stream |
| | `get_latest_frame`| Get latest frame from a session |
| **Window** | `list_windows` | List all visible applications |
| | `capture_window` | Capture a specific window by HWND |
| **Unified** | `list_all` | List both monitors and windows |
| | `capture` | Unified capture for any target |
| | `watch` | Unified streaming for any target |
| | `stop_watch` | Stop a unified stream |
| **Audio** | `list_audio_devices` | List microphones and system audio |
| | `start_audio_capture` | Start recording audio |
| | `stop_audio_capture` | Stop recording and get data |
| | `get_active_audio_sessions` | List active audio sessions |
| **AI/ML** | `listen` | Transcribe audio to text using Whisper |
| | `get_whisper_model_info` | Get information about Whisper models |

---

## Detailed Tool Reference

### Screen & Window Capture

#### `see`
Captures a screenshot of a monitor or window.
- **Arguments:**
  - `targetType` (string): "monitor" or "window" (default: "monitor")
  - `monitor` (number): Monitor index (default: 0)
  - `hwnd` (number): Window handle (required if targetType is "window")
  - `quality` (number): JPEG quality 1-100 (default: 80)
  - `maxWidth` (number): Resize image if larger than this width (default: 1920)

#### `capture` (Unified)
A single tool to capture anything.
- **Arguments:**
  - `target` (string): "primary", "monitor", "window", or "region"
  - `targetId` (string): Monitor index or HWND
  - `x`, `y`, `w`, `h` (number): Required for "region"
  - `quality` (number): JPEG quality 1-100 (default: 80)
  - `maxWidth` (number): Resize width (default: 1920)

### Streaming

#### `watch` (Unified)
- **Arguments:**
  - `target` (string): "monitor" or "window"
  - `targetId` (string): Monitor index or HWND
  - `intervalMs` (number): Capture interval (default: 1000)
  - `quality` (number): JPEG quality (default: 80)
  - `maxWidth` (number): Resize width (default: 1920)

### Audio & Speech

#### `listen`
Transcribes system or microphone audio using Whisper.
- **Arguments:**
  - `source` (string): "system", "microphone", "file", or "audio_session"
  - `sourceId` (string): File path or Audio Session ID
  - `duration` (number): Recording duration in seconds (default: 10)
  - `language` (string): "auto", "ja", "en", etc.
  - `modelSize` (string): "tiny", "base", "small", "medium", "large"
  - `translate` (boolean): Translate to English (default: false)
