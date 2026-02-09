# Changelog

All notable changes to windows-desktop-use-mcp will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **`close_window` Tool**: New tool to terminate a process by its window handle (HWND).
- **Process Identification**: Added `GetWindowThreadProcessId` P/Invoke to `InputService` to link windows to their owning processes.
- **Improved E2E Test Infrastructure**: 
    - Refactored `McpE2ETests.cs` to use `OneTimeSetUp` and `OneTimeTearDown` for better resource management.
    - Implemented a more robust Notepad window identification logic with retries and title-based fallback.
    - Added PID-based tracking to clean up only processes started during the test session.
- **Development Guidelines**: Updated `AGENTS.md` to standardize on `rg` (ripgrep) and Japanese communication for developers.
- **Incident Report**: Added `docs/report/report_20260209_notepad_e2e_fix.md` documenting the challenges with Windows 11 Notepad automation.

### Fixed
- Stabilized E2E tests by ensuring a fresh application instance is used for each suite.
- Fixed `NullReferenceException` in window enumeration when titles are null.

## [3.0.0] - 2026-02-08
...
