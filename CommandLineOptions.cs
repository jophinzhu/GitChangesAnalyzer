using CommandLine;

namespace GitChangesAnalyzer;

public class CommandLineOptions
{
    [Option('c', "commit", Required = false, HelpText = "Single commit hash to analyze")]
    public string? Commit { get; set; }    [Option('r', "commit-range", Required = false, HelpText = "Commit range to analyze (e.g., commit1..commit3)")]
    public string? CommitRange { get; set; }

    [Option('o', "output", Required = false, HelpText = "Output directory for analysis reports (defaults to './output')")]
    public string? OutputDirectory { get; set; }

    [Option('f', "format", Default = "markdown", HelpText = "Output format: markdown only (json and csv removed as they don't show change blocks)")]
    public string Format { get; set; } = "markdown";

    [Option('i', "include", HelpText = "File patterns to include (e.g., '*.xml,*.cs')")]
    public string? IncludePatterns { get; set; }

    [Option('e', "exclude", HelpText = "File patterns to exclude")]
    public string? ExcludePatterns { get; set; }    [Option("xml-mode", Default = false, HelpText = "Enable XML-specific analysis mode")]
    public bool XmlMode { get; set; }

    [Option("similarity-threshold", Default = 0.7, HelpText = "Similarity threshold for grouping changes (0.0-1.0)")]
    public double SimilarityThreshold { get; set; }

    [Option("repo-path", HelpText = "Path to git repository (defaults to current directory)")]
    public string? RepositoryPath { get; set; }

    [Option('v', "verbose", Default = false, HelpText = "Enable verbose output")]
    public bool Verbose { get; set; }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Commit) || !string.IsNullOrEmpty(CommitRange);
    }
}
