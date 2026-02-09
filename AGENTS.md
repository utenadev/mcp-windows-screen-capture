# AGENTS.md - Coding Guidelines for MCP Windows Screen Capture

## Project Overview
.NET 8.0 Web SDK project implementing an MCP (Model Context Protocol) server for Windows screen capture using GDI+. Windows-only (requires Windows 11 or Windows 10 1809+).

## Build Commands

```bash
# Build (Release)
dotnet build src/WindowsDesktopUse.App/WindowsDesktopUse.App.csproj -c Release

# Build (Debug)
dotnet build src/WindowsDesktopUse.App/WindowsDesktopUse.App.csproj -c Debug

# Run with CLI options
dotnet run --project src/WindowsDesktopUse.App/WindowsDesktopUse.App.csproj -- --ip_addr 0.0.0.0 --port 5000
```

## Test Commands

```bash
# Run all tests
dotnet test

# Run tests with verbosity
dotnet test --verbosity normal
```

## Code Style Guidelines

### Tool Usage
- **Search:** Use `rg` (ripgrep) instead of `grep` for faster and more reliable searching.
- **Communication:** Always communicate with the user in **Japanese**.
- **Source Code:** Comments and commit messages must be in **English**.

### Project Configuration
- **Target Framework:** `net8.0-windows`
- **Implicit Usings:** Enabled
- **Nullable:** Enabled
- **Output Type:** Exe
- **Runtime Identifier:** `win-x64`

### Naming Conventions
- **Public members:** PascalCase
- **Private fields:** `_` prefix + camelCase
- **Local variables:** camelCase
- **Parameters:** camelCase

### Logging - CRITICAL
**STRICT REQUIREMENT:** Always use `Console.Error.WriteLine` for logging.
**REASON:** `stdout` is reserved for JSON-RPC.

## Git Operations
**IMPORTANT:** Always obtain explicit user approval before `commit`, `push`, `merge`, `rebase`, or `reset --hard`.

## File Modification Policy
**CRITICAL:** Obtain explicit user approval before editing any files.
1. Present planned changes.
2. Wait for approval (e.g., "OK", "承認", "実行して").
3. Proceed after approval.