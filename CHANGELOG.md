# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Project organization with proper directory structure
- `.gitignore` file for .NET projects with comprehensive exclusions
- `.editorconfig` for consistent code formatting across editors
- `Directory.Build.props` for common build properties
- Test project structure with xUnit framework
- GitHub Actions CI/CD workflow (`/.github/workflows/dotnet.yml`)
- Build scripts for both Windows (`scripts/build.bat`) and Unix (`scripts/build.sh`)
- `CONTRIBUTING.md` with development guidelines
- `LICENSE` file (MIT License)
- `Makefile` for cross-platform build commands
- Comprehensive project documentation

### Changed
- Moved scripts to `scripts/` directory
- Moved documentation to `docs/` directory
- Output folder renamed from `git_analysis_reports` to `output`
- Updated README.md to reflect new project structure
- Updated solution file to include test project
- Improved code formatting in `CommandLineOptions.cs`

### Fixed
- All hardcoded references to old output folder name
- Solution file format for proper multi-project support

### Project Structure
```
GitChangesAnalyzer/
├── .github/workflows/     # CI/CD workflows
├── Core/                  # Core analysis logic
├── Models/                # Data models
├── scripts/               # Helper scripts and build tools
├── docs/                  # Documentation
├── tests/                 # Unit tests
├── output/                # Generated reports (git-ignored)
├── .gitignore            # Git ignore rules
├── .editorconfig         # Code formatting rules
├── Directory.Build.props # Common build properties
├── Makefile              # Cross-platform build commands
├── CONTRIBUTING.md       # Contribution guidelines
├── LICENSE               # MIT License
└── README.md             # Project documentation
```

## [1.0.0] - 2025-06-11

### Added
- Initial release of Git Changes Analyzer
- Support for single commit analysis
- Support for commit range analysis (up to 3 commits)
- Change pattern recognition and grouping
- XML-specific analysis mode
- Markdown report generation
- File filtering capabilities
- Configurable similarity thresholds
