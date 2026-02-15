# windows-desktop-use-mcp Tools

This document provides detailed information about the MCP tools available in this server.

## Tools Summary

| Category | Tool | Description |
|----------|------|-------------|
| **Vision** | `visual_list` | List monitors, windows, or all |
| | `visual_capture` | Capture monitor, window, or region |
| | `visual_watch` | Continuous monitoring/streaming |
| | `visual_stop` | Stop any active session |
| **Audio** | `listen` | Record and transcribe audio using Whisper |
| **Input** | `input_mouse` | Mouse operations (move, click, drag) |
| | `input_window` | Window operations (close, minimize, maximize, restore) |
| | `keyboard_key` | Press navigation keys (security restricted) |
| **Utility** | `read_window_text` | Extract window text as Markdown |

---

## Vision Tools

### `visual_list`

List monitors, windows, or all visual targets.

- **Arguments:**
  - `type` (string): "monitor", "window", or "all" (default: "all")

- **Returns:**
  - `count`: Number of items
  - `items`: Array of monitors or windows

### `visual_capture`

Capture a screenshot of a monitor, window, or region.

- **Arguments:**
  - `target` (string): "monitor", "window", "region", or "primary" (default: "primary")
  - `monitorIndex` (number): Monitor index for "monitor" target
  - `hwnd` (string): Window handle for "window" target
  - `x`, `y`, `w`, `h` (number): Region coordinates for "region" target
  - `mode` (string): "normal" (quality 30) or "detailed" (quality 70)
  - `quality` (number): JPEG quality 1-100 (default: 30 for normal, 70 for detailed)

- **Returns:**
  - Base64 encoded image data

### `visual_watch`

Start continuous monitoring or streaming of a visual target.

- **Arguments:**
  - `mode` (string): "video", "monitor", or "unified" (default: "video")
  - `target` (string): "monitor", "window", or "region"
  - `monitorIndex` (number): Monitor index for "monitor" target
  - `hwnd` (string): Window handle for "window" target
  - `x`, `y`, `w`, `h` (number): Region coordinates
  - `fps` (number): Frames per second 1-30 (default: 5)
  - `detectChanges` (boolean): Enable change detection (default: true)
  - `threshold` (number): Change threshold 0.05-0.20 (default: 0.08)

- **Returns:**
  - `sessionId`: Session ID for this watch session

### `visual_stop`

Stop any active visual or input session.

- **Arguments:**
  - `sessionId` (string): Session ID to stop (optional)
  - `type` (string): "watch", "capture", "audio", "monitor", or "all" (default: "all")

- **Returns:**
  - Confirmation message

---

## Audio Tools

### `listen`

Record system audio or microphone and transcribe using Whisper AI.

- **Arguments:**
  - `source` (string): "system", "microphone", "file", or "audio_session" (default: "system")
  - `sourceId` (string): File path or audio session ID
  - `duration` (number): Recording duration in seconds (default: 10)
  - `language` (string): "auto" or language code "ja", "en", "zh", etc. (default: "auto")
  - `modelSize` (string): "tiny", "base", "small", "medium", "large" (default: "base")
  - `translate` (boolean): Translate to English (default: false)

- **Returns:**
  - Transcribed text

---

## Input Tools

### `input_mouse`

Perform mouse operations.

- **Arguments:**
  - `action` (string): "move", "click", "drag", or "scroll"
  - `x`, `y` (number): Target coordinates
  - `button` (string): "left", "right", or "middle" (default: "left")
  - `clicks` (number): Number of clicks (default: 1)
  - `delta` (number): Scroll amount for "scroll" action

- **Returns:**
  - Confirmation message

### `input_window`

Perform window operations.

- **Arguments:**
  - `hwnd` (string): Window handle
  - `action` (string): "close", "minimize", "maximize", "restore", "activate", or "focus"

- **Returns:**
  - Confirmation message

### `keyboard_key` (Security Restricted)

Press navigation keys only. Text typing and modifier keys (Ctrl, Alt, Win) are blocked for security.

- **Arguments:**
  - `key` (string): Navigation key name
    - **Allowed:** `enter`, `return`, `tab`, `escape`, `esc`, `space`, `backspace`, `delete`, `del`, `left`, `up`, `right`, `down`, `home`, `end`, `pageup`, `pagedown`
    - **Blocked:** `ctrl`, `alt`, `win`, `shift`
  - `action` (string): "click", "press", or "release" (default: "click")

- **Returns:**
  - Confirmation message

---

## Utility Tools

### `read_window_text`

Extract text content from a window using UI Automation.

- **Arguments:**
  - `hwndStr` (string): Window handle as string
  - `includeButtons` (boolean): Include button text in output (default: false)

- **Returns:**
  - Markdown formatted text content

---

## HTTP Streaming

For `visual_watch`, frames can be streamed via HTTP:

- **Endpoint:** `http://localhost:5000/frame/{sessionId}`
- Returns the latest frame as JPEG image
