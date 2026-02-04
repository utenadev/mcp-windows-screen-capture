# Contributing to MCP Windows Screen Capture Server

Thank you for your interest in contributing to MCP Windows Screen Capture Server!

## Development Setup

### Prerequisites

- Windows 11 (or Windows 10 1809+)
- .NET 8.0 SDK
- Git

### Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/YOUR_USERNAME/mcp-windows-screen-capture.git`
3. Navigate to the project: `cd mcp-windows-screen-capture`
4. Create a new branch: `git checkout -b your-feature-branch`

### Building

```bash
# Build in Release mode
dotnet build src/WindowsScreenCaptureServer.csproj -c Release

# Build in Debug mode
dotnet build src/WindowsScreenCaptureServer.csproj -c Debug
```

### Testing

```bash
# Run all tests
dotnet test

# Run E2E tests
dotnet test tests/E2ETests/E2ETests.csproj
```

## Coding Guidelines

### Code Style

- **File-scoped classes**: No namespace braces
- **Expression-bodied members**: For simple methods
- **Target-typed new expressions**: `var list = new List<T>();`
- **Switch expressions**: For conditional logic
- **Records**: For data classes

### Naming Conventions

- **Public members**: PascalCase (e.g., `GetMonitors`)
- **Private fields**: `_` prefix + camelCase (e.g., `_monitors`)
- **Local variables**: camelCase (e.g., `capture`)
- **Parameters**: camelCase (e.g., `quality`)
- **Constants**: PascalCase

### Error Handling

- Use `try/finally` for cleanup
- Empty catch blocks only for `OperationCanceledException`
- Throw meaningful exceptions with context

### Comments

- Keep comments minimal and meaningful
- Use English for code comments
- Focus on "why" rather than "what"

## Pull Request Process

1. **Create a branch**: Use descriptive names like `fix/window-capture-issue` or `feature/new-tool`
2. **Make changes**: Follow coding guidelines
3. **Test thoroughly**: Ensure all tests pass
4. **Update documentation**: Update README and CHANGELOG as needed
5. **Commit**: Use clear commit messages
6. **Push**: Push your branch to your fork
7. **Create PR**: Use a descriptive title and fill out the PR template

### PR Template

```markdown
## Description
Brief description of changes

## Changes
- List of changes

## Testing
- How did you test these changes?

## Checklist
- [ ] Tests pass
- [ ] Documentation updated
- [ ] No breaking changes (or documented in BREAKING CHANGES)
```

## Project Structure

```
mcp-windows-screen-capture/
├── src/
│   ├── Program.cs                      # Entry point
│   ├── WindowsScreenCaptureServer.csproj
│   ├── ScreenCaptureService.cs          # Screen capture logic
│   ├── Tools/
│   │   └── ScreenCaptureTools.cs     # MCP tool definitions
│   └── bin/Release/...              # Compiled output
├── tests/
│   └── E2ETests/
│       ├── McpE2ETests.cs            # E2E tests
│       └── TestHelper.cs              # Test utilities
├── docs/                             # Documentation
├── .github/workflows/                 # CI/CD
└── README.md                          # Project README
```

## Issue Reporting

### Bug Reports

Include:
- Windows version
- .NET version
- Steps to reproduce
- Expected behavior
- Actual behavior
- Error messages/logs

### Feature Requests

Include:
- Use case
- Proposed solution
- Alternatives considered

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
