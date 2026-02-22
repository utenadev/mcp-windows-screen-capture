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
dotnet run --project src/WindowsDesktopUse.App/WindowsDesktopUse.App.csproj -- --httpPort 5000
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

<!-- bv-agent-instructions-v1 -->

---

## Beads Workflow Integration

This project uses [beads_viewer](https://github.com/Dicklesworthstone/beads_viewer) for issue tracking. Issues are stored in `.beads/` and tracked in git.

### Essential Commands

```bash
# View issues (launches TUI - avoid in automated sessions)
bv

# CLI commands for agents (use these instead)
bd ready              # Show issues ready to work (no blockers)
bd list --status=open # All open issues
bd show <id>          # Full issue details with dependencies
bd create --title="..." --type=task --priority=2
bd update <id> --status=in_progress
bd close <id> --reason="Completed"
bd close <id1> <id2>  # Close multiple issues at once
bd sync               # Commit and push changes
```

### Workflow Pattern

1. **Start**: Run `bd ready` to find actionable work
2. **Claim**: Use `bd update <id> --status=in_progress`
3. **Work**: Implement the task
4. **Complete**: Use `bd close <id>`
5. **Sync**: Always run `bd sync` at session end

### Key Concepts

- **Dependencies**: Issues can block other issues. `bd ready` shows only unblocked work.
- **Priority**: P0=critical, P1=high, P2=medium, P3=low, P4=backlog (use numbers, not words)
- **Types**: task, bug, feature, epic, question, docs
- **Blocking**: `bd dep add <issue> <depends-on>` to add dependencies

### Session Protocol

**Before ending any session, run this checklist:**

```bash
git status              # Check what changed
git add <files>         # Stage code changes
bd sync                 # Commit beads changes
git commit -m "..."     # Commit code
bd sync                 # Commit any new beads changes
git push                # Push to remote
```

### Best Practices

- Check `bd ready` at session start to find available work
- Update status as you work (in_progress → closed)
- Create new issues with `bd create` when you discover tasks
- Use descriptive titles and set appropriate priority/type
- Always `bd sync` before ending session

<!-- end-bv-agent-instructions -->
