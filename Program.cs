using CommandLine;
using LibGit2Sharp;
using GitChangesAnalyzer.Core;
using GitChangesAnalyzer.Models;

namespace GitChangesAnalyzer;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var result = await Parser.Default.ParseArguments<CommandLineOptions>(args)
            .MapResult(
                async options => await RunAnalysis(options),
                errors => Task.FromResult(1)
            );

        return result;
    }

    static async Task<int> RunAnalysis(CommandLineOptions options)
    {
        try
        {
            if (!options.IsValid())
            {
                Console.WriteLine("Error: Either --commit or --commit-range must be specified.");
                return 1;
            }

            var repositoryPath = options.RepositoryPath ?? Directory.GetCurrentDirectory();
            
            if (!Repository.IsValid(repositoryPath))
            {
                Console.WriteLine($"Error: '{repositoryPath}' is not a valid Git repository.");
                return 1;
            }            if (options.Verbose)
            {
                Console.WriteLine($"Analyzing repository at: {repositoryPath}");
                Console.WriteLine($"Output format: {options.Format}");
                Console.WriteLine($"XML mode: {options.XmlMode}");
                Console.WriteLine($"Similarity threshold: {options.SimilarityThreshold}");
                Console.WriteLine($"Output directory: {options.OutputDirectory ?? "./output"}");
            }

            using var repository = new Repository(repositoryPath);
            
            // Initialize components
            var diffParser = new GitDiffParser(repository, options.Verbose);
            var fileOrganizer = new FilePathOrganizer(options.IncludePatterns, options.ExcludePatterns, options.Verbose);
            var changeAnalyzer = new ChangeAnalyzer(options.SimilarityThreshold, options.XmlMode, options.Verbose);
            var reportGenerator = new ReportGenerator();            // Parse changes
            List<ChangeBlock> changes;
            List<string> commitHashes;
            string gitRange;
            string[] rangeParts = Array.Empty<string>();

            if (!string.IsNullOrEmpty(options.Commit))
            {
                changes = diffParser.ParseCommitDiff(options.Commit);
                commitHashes = new List<string> { options.Commit };
                gitRange = options.Commit;
            }
            else if (!string.IsNullOrEmpty(options.CommitRange))
            {
                rangeParts = options.CommitRange.Split("..", StringSplitOptions.RemoveEmptyEntries);
                if (rangeParts.Length != 2)
                {
                    Console.WriteLine("Error: Commit range must be in format 'commit1..commit2'");
                    return 1;
                }

                changes = diffParser.ParseCommitRangeDiff(rangeParts[0], rangeParts[1]);
                commitHashes = new List<string> { rangeParts[0], rangeParts[1] };
                gitRange = options.CommitRange;
            }
            else
            {
                Console.WriteLine("Error: No commit or commit range specified.");
                return 1;
            }

            if (changes.Count == 0)
            {
                Console.WriteLine("No changes found in the specified commit(s).");
                return 0;
            }

            // Filter changes based on file patterns
            changes = fileOrganizer.FilterChanges(changes);

            if (changes.Count == 0)
            {
                Console.WriteLine("No changes found after applying file filters.");
                return 0;
            }

            // Group similar changes
            var changeGroups = changeAnalyzer.GroupSimilarChanges(changes);

            // Calculate statistics
            var analysisResult = new AnalysisResult
            {
                Metadata = new AnalysisMetadata
                {
                    Timestamp = DateTime.Now,
                    TotalFiles = changes.Select(c => c.FilePath).Distinct().Count(),
                    TotalGroups = changeGroups.Count,
                    UniquePatterns = changeGroups.Count(g => g.GroupSize > 1),
                    RepositoryPath = repositoryPath,
                    CommitHashes = commitHashes,
                    GitRange = gitRange,
                    TotalChanges = changes.Count
                },
                ChangeGroups = changeGroups,
                CategoryCounts = changeGroups
                    .GroupBy(g => g.Category.ToString())
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.GroupSize))
            };            // Generate report (always markdown since it's the only format that shows change blocks)
            var report = reportGenerator.GenerateMarkdownReport(analysisResult);

            // Create output directory and filename
            var outputDirectory = options.OutputDirectory ?? "./output";
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                if (options.Verbose)
                {
                    Console.WriteLine($"Created output directory: {outputDirectory}");
                }
            }            // Generate filename with pattern: diff_analysis_<commit_id>_<timestamp>.md
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var commitId = !string.IsNullOrEmpty(options.Commit) ? 
                options.Commit.Substring(0, Math.Min(8, options.Commit.Length)) : 
                $"{rangeParts[0].Substring(0, Math.Min(8, rangeParts[0].Length))}-{rangeParts[1].Substring(0, Math.Min(8, rangeParts[1].Length))}";
            
            var fileName = $"diff_analysis_{commitId}_{timestamp}.md";
            var outputPath = Path.Combine(outputDirectory, fileName);

            // Write report to file
            await File.WriteAllTextAsync(outputPath, report);            // Display summary
            Console.WriteLine($"Analysis complete!");
            Console.WriteLine($"- Analyzed {analysisResult.Metadata.TotalChanges} changes across {analysisResult.Metadata.TotalFiles} files");
            Console.WriteLine($"- Found {analysisResult.Metadata.TotalGroups} change groups");
            Console.WriteLine($"- Identified {analysisResult.Metadata.UniquePatterns} unique patterns");
            Console.WriteLine($"- Report saved to: {outputPath}");

            if (options.Verbose)
            {
                Console.WriteLine("\nTop change patterns:");
                foreach (var group in changeGroups.Take(5).OrderByDescending(g => g.GroupSize))
                {
                    Console.WriteLine($"  - {group.Description}: {group.GroupSize} changes ({group.AffectedFiles.Count} files)");
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during analysis: {ex.Message}");
            if (options.Verbose)
            {
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            return 1;
        }
    }
}
