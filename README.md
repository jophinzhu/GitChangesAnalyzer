# GitChangesAnalyzer

A powerful tool to analyze git commit changes, intelligently group similar changes, and organize them by affected file paths to streamline code review processes.

[![.NET Build and Test](https://github.com/yourusername/GitChangesAnalyzer/actions/workflows/dotnet.yml/badge.svg)](https://github.com/yourusername/GitChangesAnalyzer/actions/workflows/dotnet.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## 🚀 Features

- **Single Commit Analysis**: Analyze changes within a specific commit
- **Multi-Commit Analysis**: Analyze changes across up to 3 commits
- **Change Pattern Recognition**: Automatically group similar changes across multiple files
- **XML-Specific Mode**: Enhanced analysis for XML file changes
- **Multiple Output Formats**: ~~Markdown, JSON, and CSV reports~~ **Markdown reports only** (other formats don't show actual change blocks needed for review)
- **File Filtering**: Include/exclude files based on patterns
- **Configurable Similarity**: Adjust similarity thresholds for grouping

## Installation

### Prerequisites
- .NET 8.0 or later
- Git repository

### Build from Source
```bash
cd "c:\Tools\GitChangesAnalyzer"
dotnet build -c Release
```

## Project Structure

```
GitChangesAnalyzer/
├── Core/                   # Core analysis logic
│   ├── ChangeAnalyzer.cs  # Change pattern detection
│   ├── FilePathOrganizer.cs # File organization logic
│   ├── GitDiffParser.cs   # Git diff parsing
│   └── ReportGenerator.cs # Report generation
├── Models/                # Data models
│   └── AnalysisModels.cs  # Analysis result models
├── scripts/               # Helper scripts
│   ├── analyze-commit.bat # Windows batch script
│   └── Analyze-Commit.ps1 # PowerShell script
├── docs/                  # Documentation
│   └── merge_request_analysis_tool_requirements.md
├── output/                # Generated reports (ignored by git)
├── CommandLineOptions.cs  # CLI configuration
├── Program.cs             # Main entry point
└── README.md             # This file
```

## Quick Start

### Using Scripts
For convenience, use the provided scripts:

**PowerShell (Recommended):**
```powershell
.\scripts\Analyze-Commit.ps1 abc123
```

**Batch:**
```batch
.\scripts\analyze-commit.bat abc123
```

**Build the project:**
```bash
.\scripts\build.sh    # Linux/macOS
.\scripts\build.bat   # Windows
```

### Direct Usage

### Basic Usage

Analyze a single commit:
```bash
dotnet run -- --commit <commit-hash>
```

Analyze multiple commits:
```bash
dotnet run -- --commit-range <commit1>..<commit3>
```

Output files are automatically generated in the `./output/` directory with the pattern:
`diff_analysis_<commit_id>_<timestamp>.md`

### Advanced Options

```bash
dotnet run -- [options]

Options:
  -c, --commit <hash>              Single commit hash to analyze
  -r, --commit-range <range>       Commit range (e.g., abc123..def456)
  -o, --output <directory>         Output directory (default: ./output)-f, --format <format>            Output format: markdown only (default: markdown)
  -i, --include <patterns>         File patterns to include (e.g., '*.xml,*.cs')
  -e, --exclude <patterns>         File patterns to exclude
  --xml-mode                       Enable XML-specific analysis
  --similarity-threshold <value>   Similarity threshold 0.0-1.0 (default: 0.8)
  --repo-path <path>              Git repository path (default: current directory)
  -v, --verbose                   Enable verbose output
  --help                          Display help
```

### Examples

#### Analyze XML files only
```bash
dotnet run -- --commit abc123 --include "*.xml" --xml-mode
```
Output: `./output/diff_analysis_abc123_20250611_143000.md`

#### Using the PowerShell script
```powershell
.\scripts\Analyze-Commit.ps1 abc123 -XmlMode -Include "*.xml,*.config"
```

#### Using the batch script
```batch
.\scripts\analyze-commit.bat abc123
```

#### Analyze with custom similarity threshold
```bash
dotnet run -- --commit-range abc123..def456 --similarity-threshold 0.9
```
Output: `./output/diff_analysis_abc123-def456_20250611_143000.md`

#### Exclude generated files with custom output directory
```bash
dotnet run -- --commit abc123 --exclude "*.designer.cs,bin/*,obj/*" --output ./reports
```
Output: `./reports/diff_analysis_abc123_20250611_143000.md`

## Output Format

### Markdown Report
The tool generates comprehensive Markdown reports that include:
- Summary statistics
- Grouped changes with diff blocks
- Affected files list  
- Category breakdown

This is the only supported format because it's the only one that shows the actual change blocks necessary for effective code review.

## Sample Output

```markdown
# Git Changes Analysis Report

## Summary
- **Total Files Changed**: 225
- **Total Change Groups**: 15
- **Unique Patterns Found**: 12
- **Analysis Date**: 2025-06-11 14:30:00

## Change Groups

### Group 1: Add XML elements (125 files)
**Similarity Score**: 95.2%
**Category**: XmlElement

#### Representative Change Block
```diff
@@ -15,6 +15,8 @@
   <configuration>
+    <appSettings>
+      <add key="NewSetting" value="DefaultValue" />
+    </appSettings>
   </configuration>
```

#### Affected Files
- Config/app.config (3 changes)
- Forms/MainForm.xml (2 changes)
- Resources/settings.xml (1 change)
...
```

## Configuration

The tool supports various configuration options through command-line arguments. For XML-heavy repositories, use `--xml-mode` for enhanced pattern recognition.

### Similarity Thresholds
- `1.0`: Exact matches only
- `0.9`: High similarity (recommended for strict grouping)
- `0.8`: Medium similarity (default)
- `0.7`: Lower similarity (more aggressive grouping)

## Performance

- **Single commits**: 1000+ files in ~30 seconds
- **Multi-commits**: Up to 3 commits in ~60 seconds
- **Memory usage**: Typically under 1GB
- **XML optimization**: Efficient handling of large XML files

## Troubleshooting

### Common Issues

1. **"Not a valid Git repository"**
   - Ensure you're in a Git repository or specify `--repo-path`

2. **"No changes found"**
   - Check if the commit hash exists
   - Verify file filters aren't too restrictive

3. **Performance issues**
   - Use file filters to reduce scope
   - Increase similarity threshold for faster grouping

### Debug Mode
Use `--verbose` for detailed analysis information:
```bash
dotnet run -- --commit abc123 --verbose
```
Output: `./output/diff_analysis_abc123_20250611_143000.md`

## Contributing

This tool is part of the CSI development toolkit. For issues or enhancements, please follow the standard CSI development process.

## License

Internal tool for CSI development use.
