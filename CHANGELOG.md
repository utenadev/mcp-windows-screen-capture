# Changelog

All notable changes to windows-desktop-use-mcp will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.9.0] - 2026-02-15

### Added
- **Unified Tool Architecture**: Consolidated fragmented tools into 9 intuitive, production-ready interfaces.
    - `visual_list`: List monitors and windows with advanced filtering.
    - `visual_capture`: Capture screenshots with dynamic quality control (Normal/Detailed).
    - `visual_watch`: Continuous monitoring and synchronized video streaming.
    - `visual_stop`: Unified session management for all visual/audio tasks.
    - `input_mouse`: Mouse operations (move, click, drag) consolidated.
    - `input_window`: Window control (close, minimize, maximize, restore).
    - `keyboard_key`: Safe navigation key operations.
    - `read_window_text`: Structural text extraction via UI Automation.
    - `listen`: System and microphone audio transcription.
- **Vision Optimization (Phase 1 & 2)**: Enhanced LLM understanding of video content.
    - **Image Overlay**: Burn [HH:MM:SS.m] timestamps and event tags directly into frames for reliable LLM OCR.
    - **Contextual Prompting**: `FrameContext` engine generates temporal prompts ("What changed since last frame?") to boost general-purpose LLM reasoning.
- **Token Efficiency Protocol**:
    - **_llm_instruction**: Priority JSON field directing LLMs to process and immediately discard heavy image data.
    - **Dynamic Quality**: Default quality set to 30 for streaming to minimize bandwidth/token usage.
- **Robustness**:
    - **GPU Capture**: `HybridCaptureService` uses `PrintWindow` with `PW_RENDERFULLCONTENT` to capture YouTube/Netflix without black screens.
    - **Type Safety**: All `hwnd` parameters standardized to `string` to prevent JSON-RPC type mismatch errors.
    - **SessionManager**: Centralized tracking of all asynchronous capture and audio sessions.

### Fixed
- Fixed hardcoded `00:00:00` timestamps in video payloads.
- Resolved cumulative timing drift in video streams using absolute-time scheduling.
- Fixed `NullReferenceException` in window enumeration when titles are null.

### Changed
- Standardized logging to `Console.Error` to keep `stdout` clean for JSON-RPC.
- Updated all E2E tests to support the new unified tool schema and JSON response formats.

## [3.0.0] - 2026-02-08
...
