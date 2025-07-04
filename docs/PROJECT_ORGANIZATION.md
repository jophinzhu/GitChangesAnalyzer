# Project Organization Guide

This document provides a comprehensive reference for the GitChangesAnalyzer project structure, configuration, and development workflow.

## 📁 Directory Structure

```
GitChangesAnalyzer/
├── .github/workflows/          # CI/CD automation
│   └── dotnet.yml             # GitHub Actions workflow
├── Core/                      # Core analysis logic
│   ├── ChangeAnalyzer.cs      # Change pattern detection
│   ├── FilePathOrganizer.cs   # File organization logic
│   ├── GitDiffParser.cs       # Git diff parsing
│   └── ReportGenerator.cs     # Report generation
├── Models/                    # Data models
│   └── AnalysisModels.cs      # Analysis result models
├── docs/                      # Documentation
│   ├── merge_request_analysis_tool_requirements.md
│   ├── ROADMAP.md             # Project roadmap
│   └── PROJECT_ORGANIZATION.md # This file
├── samples/                   # Example configurations
│   ├── README.md              # Sample documentation
│   ├── csharp-analysis.ps1    # C# analysis example
│   ├── xml-focused-analysis.bat # XML analysis example
│   ├── exclude-patterns.txt   # Common exclude patterns
│   └── include-patterns.txt   # Common include patterns
├── scripts/                   # Build and utility scripts
│   ├── Analyze-Commit.ps1     # PowerShell analysis script
│   ├── analyze-commit.bat     # Batch analysis script
│   ├── build.sh & build.bat   # Cross-platform build scripts
│   ├── clean.sh & clean.bat   # Cross-platform clean scripts
│   ├── setup.sh & setup.bat   # Cross-platform setup scripts
│   ├── docker-build.sh & .bat # Docker image build scripts
│   └── docker-run.sh & .bat   # Docker container run scripts
├── tests/                     # Unit tests
│   ├── ChangeAnalyzerTests.cs # Change analyzer tests
│   ├── GitDiffParserTests.cs  # Git diff parser tests
│   └── GitChangesAnalyzer.Tests.csproj # Test project
├── output/                    # Generated reports (git-ignored)
│   └── *.md                   # Analysis report files
├── .vscode/                   # VS Code configuration
│   ├── settings.json          # Editor settings
│   ├── tasks.json             # Build tasks
│   ├── launch.json            # Debug configuration
│   └── extensions.json        # Recommended extensions
├── Dockerfile                 # Multi-stage Docker build
├── docker-compose.yml         # Docker orchestration
├── .dockerignore              # Docker build exclusions
├── .env.example               # Environment variables template
├── .editorconfig              # Code formatting rules
├── .gitignore                 # Git ignore patterns
├── CHANGELOG.md               # Version history
├── CommandLineOptions.cs      # CLI configuration
├── CONTRIBUTING.md            # Contribution guidelines
├── Directory.Build.props      # Common build properties
├── GitChangesAnalyzer.csproj  # Main project file
├── GitChangesAnalyzer.sln     # Solution file
├── LICENSE                    # MIT License
├── Makefile                   # Cross-platform build commands
├── Program.cs                 # Main entry point
└── README.md                  # Project documentation
```

## 🔧 Configuration Files

### .gitignore
- Comprehensive .NET project exclusions
- Build artifacts (bin/, obj/, publish/)
- IDE files (.vs/, .vscode/, .idea/)
- OS files (.DS_Store, Thumbs.db)
- **Output folder properly ignored** ✅
- Generated reports and logs
- Temporary files

### .editorconfig
- Consistent code formatting across editors
- C# specific settings
- Indentation and spacing rules
- Line ending preferences

### Directory.Build.props
- Common build properties for all projects
- .NET 8.0 target framework
- Nullable reference types enabled
- Documentation generation settings

## 🚀 Build System

### Scripts
- `scripts/build.sh` & `scripts/build.bat` - Cross-platform build scripts
- `scripts/clean.sh` & `scripts/clean.bat` - Cross-platform clean scripts
- `scripts/setup.sh` & `scripts/setup.bat` - Cross-platform setup scripts
- `scripts/docker-build.sh` & `scripts/docker-build.bat` - Docker image build
- `scripts/docker-run.sh` & `scripts/docker-run.bat` - Docker container execution
- `Makefile` - Cross-platform make commands

### Docker Deployment
- `Dockerfile` - Multi-stage Docker build configuration
- `docker-compose.yml` - Container orchestration for development and production
- `.dockerignore` - Files excluded from Docker build context
- `.env.example` - Environment variables template for Docker deployment

### CI/CD
- GitHub Actions workflow (`.github/workflows/dotnet.yml`)
- Automated build, test, and publish
- Artifact generation

## 📝 Documentation

### Primary Documentation
- `README.md` - Main project documentation
- `CONTRIBUTING.md` - Development guidelines
- `CHANGELOG.md` - Version history and changes
- `LICENSE` - MIT License

### Technical Documentation
- `docs/ROADMAP.md` - Project roadmap and future plans
- `docs/merge_request_analysis_tool_requirements.md` - Original requirements
- `samples/README.md` - Sample configurations guide

## 🧪 Testing

### Test Structure
- `tests/` directory for all unit tests
- xUnit framework integration
- Separate test project (`GitChangesAnalyzer.Tests.csproj`)
- Test files follow `*Tests.cs` naming convention

### Running Tests
```bash
dotnet test
# or
./scripts/build.sh  # Includes test execution
```

## 📦 Dependencies

### Main Project
- .NET 8.0 SDK
- CommandLineParser (CLI parsing)
- LibGit2Sharp (Git operations)
- Newtonsoft.Json (JSON serialization)

### Test Project
- xUnit framework
- Microsoft.NET.Test.Sdk
- Coverage tools

## 🔒 Git Configuration

### Ignored Items ✅
- Build outputs (`bin/`, `obj/`, `publish/`)
- **Output folder (`output/`)** - Contains generated reports
- IDE configurations
- OS-specific files
- Temporary files
- Log files

### Tracked Items
- Source code files
- Configuration files
- Documentation
- Scripts and samples
- Project files

## 🎯 Key Organization Benefits

1. **Clean Repository** - Build artifacts properly ignored
2. **Professional Structure** - Industry-standard .NET layout
3. **Developer Friendly** - Clear documentation and scripts
4. **CI/CD Ready** - Automated workflows configured
5. **Cross-Platform** - Works on Windows, Linux, macOS
6. **Maintainable** - Well-organized code and documentation
7. **Extensible** - Easy to add new features and tests

## 📋 Verification Checklist

- ✅ Output folder ignored by git
- ✅ Build artifacts excluded
- ✅ Project builds successfully
- ✅ Tests can be executed
- ✅ Scripts work correctly
- ✅ Documentation is complete
- ✅ CI/CD workflow configured
- ✅ Code formatting rules applied
- ✅ License and contributing guidelines added

Last updated: June 12, 2025
