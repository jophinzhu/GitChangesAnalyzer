# Contributing to GitChangesAnalyzer

Thank you for your interest in contributing to GitChangesAnalyzer! This document provides guidelines and information for contributors.

## Development Setup

### Prerequisites
- .NET 8.0 SDK or later
- Git
- A code editor (Visual Studio, VS Code, or JetBrains Rider)

### Getting Started
1. Clone the repository
2. Run `dotnet restore` to restore dependencies
3. Run `dotnet build` to build the project
4. Run `dotnet test` to run tests

### Project Structure
```
GitChangesAnalyzer/
├── Core/                   # Core analysis logic
├── Models/                # Data models
├── scripts/               # Helper scripts
├── docs/                  # Documentation
├── tests/                 # Unit tests
└── output/                # Generated reports (git-ignored)
```

## Building the Project

### Using Build Scripts
```bash
# Windows
.\scripts\build.bat

# Linux/macOS
.\scripts\build.sh
```

### Manual Build
```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

## Code Style

The project uses EditorConfig to maintain consistent formatting. Please ensure your editor respects the `.editorconfig` file settings.

### Key Guidelines
- Use 4 spaces for indentation in C# files
- Use file-scoped namespaces
- Enable nullable reference types
- Follow standard C# naming conventions
- Add XML documentation for public APIs

## Testing

- Write unit tests for new functionality
- Ensure all tests pass before submitting a PR
- Aim for good test coverage of core logic

## Submitting Changes

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Commit your changes (`git commit -m 'Add amazing feature'`)
7. Push to your branch (`git push origin feature/amazing-feature`)
8. Open a Pull Request

## Pull Request Guidelines

- Provide a clear description of the changes
- Reference any related issues
- Ensure CI/CD pipeline passes
- Update documentation if needed
- Update CHANGELOG.md with your changes

## Reporting Issues

When reporting issues, please include:
- Operating system and version
- .NET version
- Steps to reproduce the issue
- Expected vs actual behavior
- Sample git repository or commit hash (if applicable)

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
