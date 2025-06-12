using GitChangesAnalyzer.Models;

namespace GitChangesAnalyzer.Core;

public class FilePathOrganizer
{
    private readonly List<string>? _includePatterns;
    private readonly List<string>? _excludePatterns;
    private readonly bool _verbose;

    public FilePathOrganizer(string? includePatterns = null, string? excludePatterns = null, bool verbose = false)
    {
        _includePatterns = ParsePatterns(includePatterns);
        _excludePatterns = ParsePatterns(excludePatterns);
        _verbose = verbose;
    }

    public List<ChangeBlock> FilterChanges(List<ChangeBlock> changes)
    {
        var filtered = changes.Where(ShouldIncludeFile).ToList();

        if (_verbose)
        {
            Console.WriteLine($"Filtered {changes.Count} changes to {filtered.Count} changes");
            if (_includePatterns?.Count > 0)
                Console.WriteLine($"Include patterns: {string.Join(", ", _includePatterns)}");
            if (_excludePatterns?.Count > 0)
                Console.WriteLine($"Exclude patterns: {string.Join(", ", _excludePatterns)}");
        }

        return filtered;
    }

    public Dictionary<string, List<ChangeBlock>> OrganizeByDirectory(List<ChangeBlock> changes)
    {
        var organized = new Dictionary<string, List<ChangeBlock>>();

        foreach (var change in changes)
        {
            var directory = Path.GetDirectoryName(change.FilePath) ?? "root";
            
            if (!organized.ContainsKey(directory))
                organized[directory] = new List<ChangeBlock>();
            
            organized[directory].Add(change);
        }

        return organized;
    }

    public Dictionary<string, List<ChangeBlock>> OrganizeByFileExtension(List<ChangeBlock> changes)
    {
        var organized = new Dictionary<string, List<ChangeBlock>>();

        foreach (var change in changes)
        {
            var extension = Path.GetExtension(change.FilePath).ToLower();
            if (string.IsNullOrEmpty(extension))
                extension = "no_extension";

            if (!organized.ContainsKey(extension))
                organized[extension] = new List<ChangeBlock>();
            
            organized[extension].Add(change);
        }

        return organized;
    }

    public List<string> GetRelatedFiles(string filePath, List<ChangeBlock> allChanges)
    {
        var relatedFiles = new List<string>();
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var directory = Path.GetDirectoryName(filePath);

        // Find files with same base name but different extensions
        foreach (var change in allChanges)
        {
            var changeFileName = Path.GetFileNameWithoutExtension(change.FilePath);
            var changeDirectory = Path.GetDirectoryName(change.FilePath);

            if (changeDirectory == directory && 
                changeFileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) &&
                change.FilePath != filePath)
            {
                relatedFiles.Add(change.FilePath);
            }
        }

        return relatedFiles;
    }

    private bool ShouldIncludeFile(ChangeBlock change)
    {
        var filePath = change.FilePath;

        // Check exclude patterns first
        if (_excludePatterns?.Count > 0)
        {
            foreach (var pattern in _excludePatterns)
            {
                if (MatchesPattern(filePath, pattern))
                    return false;
            }
        }

        // Check include patterns
        if (_includePatterns?.Count > 0)
        {
            foreach (var pattern in _includePatterns)
            {
                if (MatchesPattern(filePath, pattern))
                    return true;
            }
            return false; // If include patterns are specified but none match
        }

        return true; // Include by default if no patterns specified
    }

    private bool MatchesPattern(string filePath, string pattern)
    {
        // Simple pattern matching - convert glob patterns to regex
        var regexPattern = pattern
            .Replace(".", "\\.")
            .Replace("*", ".*")
            .Replace("?", ".");

        return System.Text.RegularExpressions.Regex.IsMatch(
            filePath, 
            regexPattern, 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private List<string>? ParsePatterns(string? patterns)
    {
        if (string.IsNullOrEmpty(patterns))
            return null;

        return patterns.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(p => p.Trim())
                      .ToList();
    }
}
