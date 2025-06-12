using GitChangesAnalyzer.Models;
using System.Text;

namespace GitChangesAnalyzer.Core;

public class ReportGenerator
{
    public string GenerateMarkdownReport(AnalysisResult analysisResult)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("# Git Changes Analysis Report");
        sb.AppendLine();

        // Summary
        sb.AppendLine("## Summary");
        sb.AppendLine($"- **Total Files Changed**: {analysisResult.Metadata.TotalFiles}");
        sb.AppendLine($"- **Total Change Groups**: {analysisResult.Metadata.TotalGroups}");
        sb.AppendLine($"- **Unique Patterns Found**: {analysisResult.Metadata.UniquePatterns}");
        sb.AppendLine($"- **Analysis Date**: {analysisResult.Metadata.Timestamp:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"- **Repository**: {analysisResult.Metadata.RepositoryPath}");
        
        if (analysisResult.Metadata.CommitHashes.Count > 0)
        {
            sb.AppendLine($"- **Analyzed Commits**: {string.Join(", ", analysisResult.Metadata.CommitHashes)}");
        }
        sb.AppendLine();

        // Category Summary
        if (analysisResult.CategoryCounts.Count > 0)
        {
            sb.AppendLine("## Summary by Category");
            foreach (var category in analysisResult.CategoryCounts.OrderByDescending(c => c.Value))
            {
                sb.AppendLine($"- **{category.Key}**: {category.Value} changes");
            }
            sb.AppendLine();
        }

        // Change Groups
        sb.AppendLine("## Change Groups");
        sb.AppendLine();

        var groupNumber = 1;
        foreach (var group in analysisResult.ChangeGroups.OrderByDescending(g => g.GroupSize))
        {
            sb.AppendLine($"### Group {groupNumber}: {group.Description}");
            sb.AppendLine($"**Pattern ID**: {group.PatternId}");
            sb.AppendLine($"**Similarity Score**: {group.SimilarityScore:P1}");
            sb.AppendLine($"**Files Affected**: {group.AffectedFiles.Count}");
            sb.AppendLine($"**Category**: {group.Category}");
            sb.AppendLine();            // Show representative change block
            var representativeChange = group.ChangeBlocks.First();
            sb.AppendLine("#### Representative Change Block");
            sb.AppendLine("```diff");
            sb.AppendLine(representativeChange.DiffContent); // Show complete content without truncation
            sb.AppendLine("```");
            sb.AppendLine();

            // Affected Files
            sb.AppendLine("#### Affected Files");
            foreach (var file in group.AffectedFiles.Take(20)) // Limit to first 20 files
            {
                var fileChangeCount = group.ChangeBlocks.Count(c => c.FilePath == file);
                sb.AppendLine($"- {file} ({fileChangeCount} change{(fileChangeCount > 1 ? "s" : "")})");
            }

            if (group.AffectedFiles.Count > 20)
            {
                sb.AppendLine($"- ... and {group.AffectedFiles.Count - 20} more files");
            }
            sb.AppendLine();            // Show all hunks for this group if it's small
            if (group.ChangeBlocks.Count <= 5 && group.ChangeBlocks.Count > 1)
            {
                sb.AppendLine("#### All Changes in Group");
                foreach (var change in group.ChangeBlocks)
                {
                    sb.AppendLine($"**File**: {change.FilePath}");
                    sb.AppendLine("```diff");
                    sb.AppendLine(change.DiffContent); // Show complete content without truncation
                    sb.AppendLine("```");
                }
                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine();
            groupNumber++;
        }

        return sb.ToString();
    }

    public string GenerateJsonReport(AnalysisResult analysisResult)
    {
        var jsonObject = new
        {
            analysis_metadata = new
            {
                timestamp = analysisResult.Metadata.Timestamp.ToString("O"),
                total_files = analysisResult.Metadata.TotalFiles,
                total_groups = analysisResult.Metadata.TotalGroups,
                unique_patterns = analysisResult.Metadata.UniquePatterns,
                repository_path = analysisResult.Metadata.RepositoryPath,
                commit_hashes = analysisResult.Metadata.CommitHashes,
                git_range = analysisResult.Metadata.GitRange
            },
            category_summary = analysisResult.CategoryCounts,
            change_groups = analysisResult.ChangeGroups.Select(g => new
            {
                group_id = g.PatternId,
                pattern_description = g.Description,
                similarity_score = g.SimilarityScore,
                affected_files_count = g.AffectedFiles.Count,
                affected_files = g.AffectedFiles,
                category = g.Category.ToString(),
                change_blocks = g.ChangeBlocks.Select(c => new
                {
                    file_path = c.FilePath,
                    change_type = c.ChangeType.ToString(),
                    hunk_header = c.HunkHeader,
                    diff_content = c.DiffContent, // Show complete content without truncation
                    line_numbers = new
                    {
                        start_old = c.LineNumbers.StartOld,
                        end_old = c.LineNumbers.EndOld,
                        start_new = c.LineNumbers.StartNew,
                        end_new = c.LineNumbers.EndNew
                    }
                })
            })
        };

        return System.Text.Json.JsonSerializer.Serialize(jsonObject, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    public string GenerateCsvReport(AnalysisResult analysisResult)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("Group_ID,Pattern_Description,Similarity_Score,Affected_Files_Count,Files_List,Change_Type,Category,Representative_File");

        foreach (var group in analysisResult.ChangeGroups)
        {
            var representativeChange = group.ChangeBlocks.First();
            var filesList = string.Join(";", group.AffectedFiles);
            
            sb.AppendLine($"\"{group.PatternId}\"," +
                         $"\"{EscapeCsvValue(group.Description)}\"," +
                         $"{group.SimilarityScore:F3}," +
                         $"{group.AffectedFiles.Count}," +
                         $"\"{EscapeCsvValue(filesList)}\"," +
                         $"{representativeChange.ChangeType}," +
                         $"{group.Category}," +
                         $"\"{EscapeCsvValue(representativeChange.FilePath)}\"");
        }

        return sb.ToString();
    }

    private string TruncateDiffContent(string content, int maxLines)
    {
        var lines = content.Split('\n');
        if (lines.Length <= maxLines)
            return content;

        var truncated = lines.Take(maxLines).ToArray();
        return string.Join('\n', truncated) + $"\n... (truncated, {lines.Length - maxLines} more lines)";
    }

    private string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Replace("\"", "\"\"");
    }
}
