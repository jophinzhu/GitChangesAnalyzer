# Git Changes(Diffs) Analysis Tool - Requirements Document

## Project Overview

### Purpose
Develop an automated tool to analyze changes of specific commit(s) with hundreds of files and thousands of changes, intelligently group similar changes, and organize them by affected file paths to streamline the review process.

### Problem Statement
- Commit(s) contain hundreds of files with numerous changes
- Many changes are identical or very similar across multiple files
- Manual review is time-consuming and error-prone
- Need to organize changes efficiently for systematic review
- Must ensure no changes are missed during review

### Solution Goals
1. **Automated Git Analysis**: Parse git diff output to extract all changes
2. **Change Pattern Detection**: Identify identical and similar change patterns
3. **Intelligent Grouping**: Group similar changes together with affected file lists
4. **Structured Output**: Generate organized reports for efficient review
5. **Complete Coverage**: Ensure 100% change coverage with no missed modifications

## Functional Requirements

### FR-1: Git Repository Integration
- **FR-1.1**: Connect to local git repositories
- **FR-1.2**: Execute git commands programmatically
- **FR-1.3**: Parse git diff output with full fidelity
- **FR-1.4**: Support different git diff formats (unified, side-by-side, etc.)
- **FR-1.5**: Handle large diffs efficiently (memory and performance)

### FR-2: Change Extraction and Analysis
- **FR-2.1**: Extract all hunks from git diff output for single commits or commit ranges (max 3 commits)
- **FR-2.2**: Preserve exact diff formatting and context
- **FR-2.3**: Capture file metadata (new, deleted, renamed, modified)
- **FR-2.4**: Special handling for XML file changes with proper indentation preservation
- **FR-2.5**: Support analysis of both `git show <commit>` and `git diff <commit1>..<commit3>` outputs

### FR-3: Change Pattern Recognition

- **FR-3.1**: Identify identical change blocks across multiple files
- **FR-3.2**: Detect similar changes with minor variations
- **FR-3.3**: Group changes by:
  - Exact match (100% identical)
  - Pattern similarity (90%+ similar)
  - Change type (additions, deletions, modifications)
  - Functional category (XML elements, attributes, C# methods, imports)
  - XML-specific patterns (element additions, attribute changes, text content updates)
- **FR-3.4**: Calculate similarity scores for change grouping
- **FR-3.5**: Special XML change recognition for:
  - Element hierarchy changes
  - Attribute value modifications
  - XML namespace additions/changes
  - CDATA section modifications

### FR-4: File Path Organization
- **FR-4.1**: List all affected files for each change group
- **FR-4.2**: Organize files by directory structure
- **FR-4.3**: Group related files (e.g., .cs and .designer.cs pairs)
- **FR-4.4**: Support configurable path filtering and grouping rules

### FR-5: Output Generation
- **FR-5.1**: Generate structured markdown reports
- **FR-5.2**: Create tabular output with change blocks and affected files
- **FR-5.3**: Provide summary statistics
- **FR-5.4**: Export to CSV format

## Technical Requirements

### TR-1: Git Command Integration
```bash
# Required git commands to support:
# For single commit analysis
git show --unified=5 <commit-hash>
git show --name-status <commit-hash>
git show --stat <commit-hash>

# For multiple commits (up to 3)
git diff --unified=5 <commit1>^..<commit3>
git diff --name-status <commit1>^..<commit3>
git diff --stat <commit1>^..<commit3>

# Get commit information
git log --oneline -n 3 <commit-range>
git log --pretty=format:"%H %s %an %ad" <commit-range>
```

### TR-2: Change Pattern Algorithms
- **TR-2.1**: Implement fuzzy string matching for change similarity
- **TR-2.2**: Use diff algorithms (Myers, Patience, Histogram)
- **TR-2.3**: Apply semantic analysis for code changes
- **TR-2.4**: Support configurable similarity thresholds

### TR-3: Data Structures
```
ChangeBlock:
- diff_content: string (exact git diff output)
- file_path: string
- hunk_header: string (@@ line info @@)
- change_type: enum (add, delete, modify)
- line_numbers: {start_old, end_old, start_new, end_new}
- context_lines: array of strings
- similarity_hash: string

ChangeGroup:
- pattern_id: string
- similarity_score: float
- change_blocks: array of ChangeBlock
- affected_files: array of strings
- description: string
- category: enum (feature, bugfix, refactor, etc.)
```

### TR-4: Performance Requirements

- **TR-4.1**: Process single commits with 1000+ files within 30 seconds
- **TR-4.2**: Handle multi-commit analysis (up to 3 commits) within 60 seconds
- **TR-4.3**: Handle XML files up to 50MB size efficiently
- **TR-4.4**: Memory usage under 1GB for typical commit analysis
- **TR-4.5**: XML parsing and pattern recognition optimized for large XML files

## Output Format Specification

### Primary Output: Organized Change Report

```markdown
# Git Changes(Diffs) Analysis Report

## Summary
- **Total Files Changed**: {count}
- **Total Change Groups**: {count}
- **Unique Patterns Found**: {count}
- **Analysis Date**: {timestamp}

## Change Groups

### Group 1: {Pattern Description}
**Similarity Score**: {percentage}%
**Files Affected**: {count}

#### Change Block
```diff
{exact git diff output}
```

#### Affected Files
- path/to/file1.cs
- path/to/file2.cs
- path/to/file3.cs

---

### Group 2: {Pattern Description}
{repeat format}
```

### Secondary Outputs

#### JSON Export
```json
{
  "analysis_metadata": {
    "timestamp": "2025-06-11T10:00:00Z",
    "total_files": 225,
    "total_groups": 15,
    "git_range": "main...feature-branch"
  },
  "change_groups": [
    {
      "group_id": "group_001",
      "pattern_description": "Add import statements",
      "similarity_score": 0.95,
      "change_block": "git diff content",
      "affected_files": ["file1.cs", "file2.cs"],
      "category": "imports",
      "risk_level": "low"
    }
  ]
}
```

#### CSV Export
```csv
Group_ID,Pattern_Description,Similarity_Score,Affected_Files_Count,Files_List,Change_Type,Risk_Level
group_001,"Add import statements",0.95,3,"file1.cs;file2.cs;file3.cs",addition,low
```

## Implementation Architecture

### Core Components

1. **GitDiffParser**
   - Parse git diff output
   - Extract hunks and metadata
   - Handle various diff formats

2. **ChangeAnalyzer**
   - Identify change patterns
   - Calculate similarity scores
   - Group related changes

3. **FilePathOrganizer**
   - Organize files by patterns
   - Handle directory structures
   - Apply grouping rules

4. **ReportGenerator**
   - Generate formatted outputs
   - Apply templates
   - Export to multiple formats

5. **ConfigurationManager**
   - Similarity thresholds
   - Grouping rules
   - Output templates
   - File filtering patterns

### Technology Stack Options

#### Option 1: C# (.NET)
- Leverage existing GitProcessor.cs patterns
- Use LibGit2Sharp for git operations
- Strong typing and performance
- Integration with existing codebase

#### Option 2: Python
- Rich ecosystem for text processing
- Libraries: GitPython, difflib, pandas
- Excellent for pattern recognition
- Easy prototyping and iteration

#### Option 3: PowerShell
- Native Windows integration
- Good for git command execution
- Built-in text processing
- Easy deployment

## Usage Scenarios

### Scenario 1: Single Commit Analysis
```bash
# Analyze a specific commit
./commit-analyzer --commit <commit-hash> --output report.md

# Expected output: Grouped changes within that commit with file lists
# Example: "Update XML node values" affects 25 XML files
```

### Scenario 2: Multi-Commit Analysis (up to 3 commits)
```bash
# Analyze a range of commits
./commit-analyzer --commit-range <commit1>..<commit3> --output report.md

# Expected output: Groups like "Add XML configuration elements" across multiple commits
```

### Scenario 3: XML-Focused Analysis
```bash
# Focus specifically on XML file changes
./commit-analyzer --commit <commit-hash> --include "*.xml" --xml-mode

# Expected output: XML-specific change patterns with proper element/attribute grouping
```

## Configuration Options

### Similarity Thresholds
```json
{
  "similarity_thresholds": {
    "exact_match": 1.0,
    "high_similarity": 0.9,
    "medium_similarity": 0.7,
    "low_similarity": 0.5
  },
  "grouping_rules": {
    "max_group_size": 100,
    "min_group_size": 2,
    "merge_similar_groups": true
  }
}
```

### File Filtering
```json
{
  "file_filters": {
    "include_patterns": ["*.xml", "*.cs", "*.config"],
    "exclude_patterns": ["*.designer.cs", "bin/*", "obj/*"],
    "group_extensions": true,
    "xml_specific_patterns": ["*.xml", "*.config", "*.settings"],
    "group_directories": ["Forms", "Config", "Resources"]
  }
}
```

### Output Customization
```json
{
  "output_options": {
    "include_context_lines": 3,
    "max_diff_size": 100,
    "truncate_large_diffs": true,
    "include_file_stats": true,
    "group_by_directory": false
  }
}
```

## Quality Assurance

### Validation Requirements
- **VAL-1**: Verify 100% change coverage (no missed hunks)
- **VAL-2**: Validate diff fidelity (exact reproduction)
- **VAL-3**: Test similarity algorithm accuracy
- **VAL-4**: Ensure consistent grouping results
- **VAL-5**: Performance benchmarking with large repositories

### Test Cases

1. **Single XML commit (10-20 files)**: Verify basic XML change pattern recognition
2. **Multi-commit XML changes (50-100 files)**: Test grouping accuracy across commits
3. **Mixed XML/CS commit (100+ files)**: Performance and mixed file type testing
4. **Large XML generation commit**: Pattern recognition for auto-generated XML
5. **XML configuration updates**: Various XML attribute and element change patterns

## Success Criteria

### Primary Goals
- ✅ **Completeness**: 100% of changes captured and analyzed
- ✅ **Efficiency**: 80%+ reduction in review time
- ✅ **Accuracy**: 90%+ correct pattern grouping
- ✅ **Usability**: Simple command-line interface
- ✅ **Performance**: Handle 1000+ files in under 60 seconds

### Secondary Goals
- ✅ **Flexibility**: Configurable grouping and filtering
- ✅ **Integration**: Work with existing git workflows
- ✅ **Reporting**: Professional, readable output formats
- ✅ **Maintenance**: Extensible architecture for future needs

## Delivery Timeline

### Phase 1: Core Functionality (Week 1-2)
- Git diff parsing
- Basic change extraction
- Simple pattern matching
- Markdown output generation

### Phase 2: Advanced Grouping (Week 3-4)
- Similarity algorithms
- Intelligent change grouping
- File path organization
- Configuration system

### Phase 3: Polish and Testing (Week 5-6)
- Multiple output formats
- Performance optimization
- Comprehensive testing
- Documentation and examples

## Risk Assessment

### Technical Risks
- **High**: Performance with very large diffs
- **Medium**: Accuracy of similarity detection
- **Low**: Git command integration complexity

### Mitigation Strategies
- Implement streaming diff processing
- Provide configurable similarity thresholds
- Use proven git libraries and tools
- Extensive testing with real-world data

## Future Enhancements

### Version 2.0 Features
- Web-based interface
- Integration with GitLab/GitHub
- AI-powered change categorization
- Automated risk assessment
- Team collaboration features

### Integration Opportunities
- CI/CD pipeline integration
- Code review tool plugins
- IDE extensions
- Automated testing triggers

---

*This requirements document serves as the foundation for developing a comprehensive git changes analysis tool that will significantly improve the efficiency and accuracy of code review processes.*
