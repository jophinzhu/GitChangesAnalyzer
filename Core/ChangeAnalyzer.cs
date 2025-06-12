using GitChangesAnalyzer.Models;
using System.Text.RegularExpressions;

namespace GitChangesAnalyzer.Core;

public class ChangeAnalyzer
{
    private readonly double _similarityThreshold;
    private readonly bool _xmlMode;
    private readonly bool _verbose;

    public ChangeAnalyzer(double similarityThreshold = 0.7, bool xmlMode = false, bool verbose = false)
    {
        _similarityThreshold = similarityThreshold;
        _xmlMode = xmlMode;
        _verbose = verbose;
    }    public List<ChangeGroup> GroupSimilarChanges(List<ChangeBlock> changes)
    {
        var groups = new List<ChangeGroup>();
        var processedChanges = new HashSet<ChangeBlock>();

        if (_verbose)
            Console.WriteLine($"Analyzing {changes.Count} changes for similarity grouping...");

        // First pass: Group complete XML node changes (deletions + insertions) within the same file
        if (_xmlMode)
        {
            GroupCompleteXmlNodeChanges(changes, groups, processedChanges);
        }

        // Second pass: Group remaining similar changes
        foreach (var change in changes)
        {
            if (processedChanges.Contains(change))
                continue;

            var similarChanges = FindSimilarChanges(change, changes, processedChanges);
            
            if (similarChanges.Count >= 2) // At least 2 similar changes to form a group
            {
                var group = CreateChangeGroup(similarChanges);
                groups.Add(group);
                
                foreach (var similarChange in similarChanges)
                {
                    processedChanges.Add(similarChange);
                }

                if (_verbose)
                    Console.WriteLine($"Created group '{group.Description}' with {group.GroupSize} changes");
            }
        }

        // Add remaining single changes as individual groups
        var remainingChanges = changes.Where(c => !processedChanges.Contains(c)).ToList();
        foreach (var change in remainingChanges)
        {
            var singleGroup = CreateChangeGroup(new List<ChangeBlock> { change });
            groups.Add(singleGroup);
        }

        if (_verbose)
            Console.WriteLine($"Created {groups.Count} total groups");

        return groups.OrderByDescending(g => g.GroupSize).ToList();
    }private List<ChangeBlock> FindSimilarChanges(ChangeBlock targetChange, List<ChangeBlock> allChanges, HashSet<ChangeBlock> processedChanges)
    {
        var identicalChanges = new List<ChangeBlock> { targetChange };

        foreach (var change in allChanges)
        {
            if (change == targetChange || processedChanges.Contains(change))
                continue;

            // Only group IDENTICAL changes (100% match)
            if (AreChangesIdentical(targetChange, change))
            {
                identicalChanges.Add(change);
            }
        }

        return identicalChanges;
    }    private bool AreChangesIdentical(ChangeBlock change1, ChangeBlock change2)
    {
        // Quick check for exact matches using similarity hash
        if (change1.SimilarityHash == change2.SimilarityHash)
            return true;

        // Additional check: normalize and compare the actual changed content
        var normalized1 = NormalizeContent(change1.DiffContent);
        var normalized2 = NormalizeContent(change2.DiffContent);

        // Only group if the normalized changed content is EXACTLY the same
        return normalized1 == normalized2;
    }

    private string NormalizeContent(string content)
    {
        // Extract ONLY the actual changed lines (+ and -), ignore context lines
        var lines = content.Split('\n')
            .Where(line => line.StartsWith("+") || line.StartsWith("-"))
            .Where(line => !line.StartsWith("+++") && !line.StartsWith("---"))
            .Select(line => line.Substring(1).Trim()) // Remove +/- marker and normalize whitespace
            .Where(line => !string.IsNullOrWhiteSpace(line));

        var normalized = string.Join(" ", lines);

        // Additional XML normalization if in XML mode
        if (_xmlMode)
        {
            normalized = NormalizeXmlContent(normalized);
        }

        return normalized;
    }

    private string NormalizeXmlContent(string content)
    {        // Normalize XML attributes and values for better pattern matching
        content = Regex.Replace(content, @"id\s*=\s*""[^""]*""", @"id=""""", RegexOptions.IgnoreCase);
        content = Regex.Replace(content, @"value\s*=\s*""[^""]*""", @"value=""""", RegexOptions.IgnoreCase);
        content = Regex.Replace(content, @"name\s*=\s*""[^""]*""", @"name=""""", RegexOptions.IgnoreCase);
        
        // AGGRESSIVE normalization for Post301Format AUTOIME(NoControl) removals
        // Treat ANY removal of AUTOIME(NoControl) from Post301Format as identical
        if (content.ToLower().Contains("post301format") && content.ToLower().Contains("autoime(nocontrol)"))
        {
            // Normalize ALL Post301Format AUTOIME removals to the same pattern
            content = "POST301FORMAT_AUTOIME_REMOVAL_PATTERN";
        }
        
        // AGGRESSIVE normalization for DefaultFrom() removals
        if (content.ToLower().Contains("defaultfrom") && content.ToLower().Contains("()"))
        {
            content = "DEFAULTFROM_EMPTY_REMOVAL_PATTERN";
        }
        
        // Normalize LayoutAttributes CONFIG patterns to group similar configurations
        content = Regex.Replace(content, @"CONFIG\(\{[^}]*\}\)", @"CONFIG({...})", RegexOptions.IgnoreCase);
        
        // Normalize ContainerSequence number changes (treat all number changes as similar)
        content = Regex.Replace(content, @"<ContainerSequence>\d+</ContainerSequence>", @"<ContainerSequence>N</ContainerSequence>", RegexOptions.IgnoreCase);
        
        // Normalize numeric values in quotes (like "1" vs 1 in JSON-like attributes)
        content = Regex.Replace(content, @"""(\d+)""", @"""N""", RegexOptions.IgnoreCase);
        
        // Normalize DeviceID numbers
        content = Regex.Replace(content, @"<DeviceID>-?\d+</DeviceID>", @"<DeviceID>N</DeviceID>", RegexOptions.IgnoreCase);
        
        // Normalize component names that are auto-generated
        content = Regex.Replace(content, @"<Component Name=""[^""]*""", @"<Component Name=""NAME""", RegexOptions.IgnoreCase);
        
        return content;
    }

    private ChangeGroup CreateChangeGroup(List<ChangeBlock> changes)
    {
        var firstChange = changes.First();
        var affectedFiles = changes.Select(c => c.FilePath).Distinct().ToList();
        
        var group = new ChangeGroup
        {
            PatternId = Guid.NewGuid().ToString("N")[..8],
            ChangeBlocks = changes,
            AffectedFiles = affectedFiles,
            SimilarityScore = changes.Count > 1 ? CalculateGroupSimilarity(changes) : 1.0,
            Category = DetermineCategory(firstChange),
            Description = GenerateDescription(changes)
        };

        return group;
    }    private double CalculateGroupSimilarity(List<ChangeBlock> changes)
    {
        // Since we only group identical changes, similarity is always 100%
        return 1.0;
    }

    private ChangeCategory DetermineCategory(ChangeBlock change)
    {
        var filePath = change.FilePath.ToLower();
        var content = change.DiffContent.ToLower();

        if (filePath.EndsWith(".xml") || filePath.EndsWith(".config"))
        {
            if (content.Contains("<") && content.Contains(">"))
            {
                if (content.Contains("attribute") || content.Contains("="))
                    return ChangeCategory.XmlAttribute;
                else
                    return ChangeCategory.XmlElement;
            }
            return ChangeCategory.XmlContent;
        }

        if (filePath.EndsWith(".cs"))
        {
            if (content.Contains("using ") || content.Contains("import "))
                return ChangeCategory.CSharpImport;
            if (content.Contains("public ") && (content.Contains("class ") || content.Contains("method ") || content.Contains("()")))
                return ChangeCategory.CSharpMethod;
            if (content.Contains("{ get") || content.Contains("{ set"))
                return ChangeCategory.CSharpProperty;
        }

        if (filePath.Contains("config") || filePath.EndsWith(".json"))
            return ChangeCategory.ConfigurationChange;

        return ChangeCategory.Other;    }
      private string GenerateDescription(List<ChangeBlock> changes)
    {
        var firstChange = changes.First();
        var fileCount = changes.Select(c => c.FilePath).Distinct().Count();
        var changeTypes = changes.Select(c => c.ChangeType).Distinct().ToList();

        // Check if this is a combined deletion/insertion group (XML node restructuring)
        if (changeTypes.Contains(ChangeType.Delete) && changeTypes.Contains(ChangeType.Add))
        {
            var deletions = changes.Where(c => c.ChangeType == ChangeType.Delete).Count();
            var additions = changes.Where(c => c.ChangeType == ChangeType.Add).Count();
              // For XML files, this likely represents node restructuring
            if (IsXmlFile(firstChange.FilePath))
            {
                var xmlDescription = AnalyzeXmlNodeRestructuring(changes);
                if (fileCount > 1)
                    xmlDescription += $" ({fileCount} files)";
                return xmlDescription;
            }
            
            return $"Restructure code elements - {deletions} deletions, {additions} additions ({fileCount} files)";
        }

        // Original logic for single change types
        var changeType = firstChange.ChangeType;

        // Analyze the actual diff content to generate accurate descriptions
        var diffContent = firstChange.DiffContent.ToLower();
        var addedLines = diffContent.Split('\n').Where(line => line.StartsWith("+") && !line.StartsWith("+++")).ToList();
        var removedLines = diffContent.Split('\n').Where(line => line.StartsWith("-") && !line.StartsWith("---")).ToList();

        string description;

        // Specific XML pattern detection
        if (IsXmlFile(firstChange.FilePath))
        {
            description = AnalyzeXmlChanges(addedLines, removedLines, changeType);
        }
        else
        {
            // Generic description for non-XML files
            var category = DetermineCategory(firstChange);
            description = category switch
            {
                ChangeCategory.CSharpImport => $"{changeType} using statements",
                ChangeCategory.CSharpMethod => $"{changeType} C# methods",
                ChangeCategory.CSharpProperty => $"{changeType} C# properties",
                ChangeCategory.ConfigurationChange => $"{changeType} configuration values",
                _ => $"{changeType} code changes"
            };
        }

        if (fileCount > 1)
            description += $" ({fileCount} files)";

        return description;
    }

    private string AnalyzeXmlNodeRestructuring(List<ChangeBlock> changes)
    {
        // Analyze the combined changes to understand what kind of restructuring occurred
        var deletedContent = string.Join(" ", changes
            .Where(c => c.ChangeType == ChangeType.Delete)
            .SelectMany(c => c.DiffContent.Split('\n')
                .Where(line => line.StartsWith("-") && !line.StartsWith("---"))
                .Select(line => line.Substring(1).Trim())));

        var addedContent = string.Join(" ", changes
            .Where(c => c.ChangeType == ChangeType.Add)
            .SelectMany(c => c.DiffContent.Split('\n')
                .Where(line => line.StartsWith("+") && !line.StartsWith("+++"))
                .Select(line => line.Substring(1).Trim())));

        // Detect specific patterns
        if (deletedContent.ToLower().Contains("<component") && addedContent.ToLower().Contains("<component"))
        {
            return "Restructure Component XML elements";
        }

        if (deletedContent.ToLower().Contains("defaultfrom") || addedContent.ToLower().Contains("defaultfrom"))
        {
            return "Restructure DefaultFrom XML elements";
        }

        if (deletedContent.ToLower().Contains("post301format") || addedContent.ToLower().Contains("post301format"))
        {
            return "Restructure Post301Format XML elements";
        }

        if (deletedContent.ToLower().Contains("layoutattributes") || addedContent.ToLower().Contains("layoutattributes"))
        {
            return "Restructure LayoutAttributes XML elements";
        }

        // Generic XML restructuring
        return "Restructure XML elements";
    }

    private string AnalyzeXmlChanges(List<string> addedLines, List<string> removedLines, ChangeType changeType)
    {
        // Check for specific XML patterns
        
        // Check for DefaultFrom removal
        if (removedLines.Any(line => line.Contains("<defaultfrom>()") || line.Contains("<defaultfrom>()</defaultfrom>")))
        {
            return "Remove empty DefaultFrom elements";
        }

        // Check for Post301Format AUTOIME removal
        if (removedLines.Any(line => line.Contains("post301format") && line.Contains("autoime(nocontrol)")))
        {
            if (addedLines.Any(line => line.Contains("post301format") && line.Contains("/>")))
            {
                return "Remove AUTOIME(NoControl) from Post301Format elements";
            }
            else if (addedLines.Any(line => line.Contains("post301format") && !line.Contains("autoime(nocontrol)")))
            {
                return "Remove AUTOIME(NoControl) from Post301Format elements";
            }
        }

        // Check for LayoutAttributes CONFIG changes
        if (addedLines.Any(line => line.Contains("layoutattributes") && line.Contains("config(")) ||
            removedLines.Any(line => line.Contains("layoutattributes") && line.Contains("config(")))
        {
            return "Update LayoutAttributes CONFIG settings";
        }

        // Check for ContainerSequence changes
        if (addedLines.Any(line => line.Contains("containersequence")) ||
            removedLines.Any(line => line.Contains("containersequence")))
        {
            return "Update ContainerSequence values";
        }

        // Check for XML attribute changes
        if (addedLines.Any(line => line.Contains("=\"")) || removedLines.Any(line => line.Contains("=\"")))
        {
            return "Modify XML attributes";
        }

        // Check for XML element changes
        if (addedLines.Any(line => line.Contains("<") && line.Contains(">")) || 
            removedLines.Any(line => line.Contains("<") && line.Contains(">")))
        {
            return "Modify XML elements";
        }

        // Fallback based on change type
        return changeType switch
        {
            ChangeType.Add => "Add XML content",
            ChangeType.Delete => "Remove XML content",
            ChangeType.Modify => "Modify XML content",
            _ => "Update XML content"
        };
    }

    private bool IsXmlFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        return new[] { ".xml", ".config", ".settings", ".resx", ".xaml" }.Contains(extension);
    }

    private bool IsXmlRelated(string extension)
    {
        var xmlExtensions = new[] { ".xml", ".config", ".settings", ".resx", ".xaml" };
        return xmlExtensions.Contains(extension.ToLower());
    }    private void GroupCompleteXmlNodeChanges(List<ChangeBlock> changes, List<ChangeGroup> groups, HashSet<ChangeBlock> processedChanges)
    {
        // Group changes by file path
        var changesByFile = changes
            .Where(c => !processedChanges.Contains(c))
            .GroupBy(c => c.FilePath)
            .ToList();

        foreach (var fileGroup in changesByFile)
        {
            var fileChanges = fileGroup.ToList();
            var deletions = fileChanges.Where(c => c.ChangeType == ChangeType.Delete).ToList();
            var additions = fileChanges.Where(c => c.ChangeType == ChangeType.Add).ToList();

            // More aggressive grouping: Look for any substantial XML changes in the same file
            var substantialDeletions = deletions.Where(d => IsSubstantialXmlChange(d.DiffContent)).ToList();
            var substantialAdditions = additions.Where(a => IsSubstantialXmlChange(a.DiffContent)).ToList();

            // If we have both substantial deletions and additions in the same file, group them
            if (substantialDeletions.Any() && substantialAdditions.Any())
            {
                var combinedChanges = new List<ChangeBlock>();
                combinedChanges.AddRange(substantialDeletions);
                combinedChanges.AddRange(substantialAdditions);

                var group = CreateChangeGroup(combinedChanges);
                groups.Add(group);

                // Mark all as processed
                foreach (var change in combinedChanges)
                {
                    processedChanges.Add(change);
                }

                if (_verbose)
                    Console.WriteLine($"Created XML restructuring group '{group.Description}' with {group.GroupSize} changes ({substantialDeletions.Count} deletions + {substantialAdditions.Count} additions)");
            }
            else
            {
                // Original logic for individual complete XML nodes
                foreach (var deletion in deletions)
                {
                    if (IsCompleteXmlNode(deletion.DiffContent))
                    {
                        // Find corresponding additions that might be related
                        var relatedAdditions = additions
                            .Where(a => IsCompleteXmlNode(a.DiffContent) && 
                                       AreRelatedXmlNodes(deletion.DiffContent, a.DiffContent))
                            .ToList();

                        if (relatedAdditions.Any())
                        {
                            // Create a group for the deletion and related additions
                            var combinedChanges = new List<ChangeBlock> { deletion };
                            combinedChanges.AddRange(relatedAdditions);

                            var group = CreateChangeGroup(combinedChanges);
                            groups.Add(group);

                            // Mark all as processed
                            foreach (var change in combinedChanges)
                            {
                                processedChanges.Add(change);
                            }

                            if (_verbose)
                                Console.WriteLine($"Created XML node group '{group.Description}' with {group.GroupSize} changes (deletion + insertion)");
                        }
                    }
                }
            }
        }
    }

    private bool IsSubstantialXmlChange(string diffContent)
    {
        // Count the number of changed lines
        var changedLines = diffContent.Split('\n')
            .Where(line => line.StartsWith("+") || line.StartsWith("-"))
            .Where(line => !line.StartsWith("+++") && !line.StartsWith("---"))
            .Count();

        // Consider it substantial if it has more than 8 lines of changes
        if (changedLines > 8)
        {
            return true;
        }

        // Also check if it contains complete XML elements
        return IsCompleteXmlNode(diffContent);
    }private bool IsCompleteXmlNode(string diffContent)
    {
        // Extract the actual changed lines
        var lines = diffContent.Split('\n')
            .Where(line => line.StartsWith("+") || line.StartsWith("-"))
            .Where(line => !line.StartsWith("+++") && !line.StartsWith("---"))
            .Select(line => line.Substring(1).Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        // Check if this is a substantial change (more than 10 lines or contains complete XML structure)
        if (lines.Count >= 10)
        {
            return true; // Large changes are likely complete XML structures
        }

        var content = string.Join(" ", lines).Trim();

        // Check if this represents a complete XML node
        return IsCompleteXmlElement(content) || IsLargeXmlBlock(content);
    }

    private bool IsLargeXmlBlock(string content)
    {
        // Consider it a large XML block if:
        // 1. It has multiple XML elements (more than 3 lines with XML tags)
        // 2. It contains nested structure
        var xmlLines = content.Split(' ')
            .Where(part => part.Contains("<") && part.Contains(">"))
            .Count();

        if (xmlLines >= 3) // At least 3 XML elements
        {
            return true;
        }

        // Check for nested structure patterns
        if (content.Contains("<Component") && content.Contains("</Component>"))
        {
            return true;
        }

        // Check for multiple nested elements
        var openTags = Regex.Matches(content, @"<[A-Za-z][A-Za-z0-9]*[^/>]*>", RegexOptions.IgnoreCase).Count;
        var closeTags = Regex.Matches(content, @"</[A-Za-z][A-Za-z0-9]*>", RegexOptions.IgnoreCase).Count;

        // If we have multiple open/close tag pairs, it's likely a complete structure
        return openTags >= 2 && closeTags >= 1;
    }

    private bool IsCompleteXmlElement(string content)
    {
        // Common XML elements we want to detect as complete nodes
        var xmlElementPatterns = new[]
        {
            @"<Component\s+[^>]*>.*?</Component>",
            @"<Component\s*>.*?</Component>",
            @"<Element\s+[^>]*>.*?</Element>",
            @"<Element\s*>.*?</Element>",
            @"<[A-Za-z][A-Za-z0-9]*\s+[^>]*>.*?</[A-Za-z][A-Za-z0-9]*>",
            @"<[A-Za-z][A-Za-z0-9]*\s*>.*?</[A-Za-z][A-ZaZ0-9]*>"
        };

        // Check if content matches any complete XML element pattern
        foreach (var pattern in xmlElementPatterns)
        {
            if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                return true;
            }
        }

        // Also check for self-closing tags that represent complete elements
        if (Regex.IsMatch(content, @"<[A-Za-z][A-Za-z0-9]*[^>]*/\s*>", RegexOptions.IgnoreCase))
        {
            return true;
        }

        // Check for multiple lines that form a complete element
        var trimmedContent = content.Replace(" ", "");
        
        // Look for opening and closing tags of the same element
        var openTagMatch = Regex.Match(trimmedContent, @"<([A-Za-z][A-ZaZ0-9]*)[^>]*>", RegexOptions.IgnoreCase);
        if (openTagMatch.Success)
        {
            var elementName = openTagMatch.Groups[1].Value;
            var closingTag = $"</{elementName}>";
            if (trimmedContent.ToLower().Contains(closingTag.ToLower()))
            {
                return true;
            }
        }

        return false;
    }

    private bool AreRelatedXmlNodes(string deletedContent, string addedContent)
    {
        // Extract element names from both contents
        var deletedElementName = ExtractXmlElementName(deletedContent);
        var addedElementName = ExtractXmlElementName(addedContent);

        // If they have the same element name, they're likely related
        if (!string.IsNullOrEmpty(deletedElementName) && !string.IsNullOrEmpty(addedElementName))
        {
            return deletedElementName.Equals(addedElementName, StringComparison.OrdinalIgnoreCase);
        }

        // Additional heuristics for related nodes
        var normalizedDeleted = NormalizeXmlForComparison(deletedContent);
        var normalizedAdded = NormalizeXmlForComparison(addedContent);

        // Check if they have similar structure or attributes
        var similarity = CalculateStringSimilarity(normalizedDeleted, normalizedAdded);
        return similarity > 0.3; // At least 30% similar
    }

    private string ExtractXmlElementName(string xmlContent)
    {
        // Extract the actual changed lines
        var lines = xmlContent.Split('\n')
            .Where(line => line.StartsWith("+") || line.StartsWith("-"))
            .Where(line => !line.StartsWith("+++") && !line.StartsWith("---"))
            .Select(line => line.Substring(1).Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var content = string.Join(" ", lines);

        // Look for the first XML element name
        var match = Regex.Match(content, @"<([A-Za-z][A-Za-z0-9]*)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private string NormalizeXmlForComparison(string xmlContent)
    {
        // Remove diff markers and normalize
        var lines = xmlContent.Split('\n')
            .Where(line => line.StartsWith("+") || line.StartsWith("-"))
            .Where(line => !line.StartsWith("+++") && !line.StartsWith("---"))
            .Select(line => line.Substring(1).Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var content = string.Join(" ", lines);

        // Normalize attributes and values for comparison
        content = Regex.Replace(content, @"id\s*=\s*""[^""]*""", @"id=""ID""", RegexOptions.IgnoreCase);
        content = Regex.Replace(content, @"name\s*=\s*""[^""]*""", @"name=""NAME""", RegexOptions.IgnoreCase);
        content = Regex.Replace(content, @"""[^""]*""", @"""VALUE""", RegexOptions.IgnoreCase);

        return content;
    }

    private double CalculateStringSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            return 0.0;

        if (str1 == str2)
            return 1.0;

        var maxLength = Math.Max(str1.Length, str2.Length);
        var distance = LevenshteinDistance(str1, str2);
        
        return maxLength == 0 ? 0.0 : 1.0 - ((double)distance / maxLength);
    }

    private int LevenshteinDistance(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1)) return s2?.Length ?? 0;
        if (string.IsNullOrEmpty(s2)) return s1.Length;

        var matrix = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
            matrix[i, 0] = i;

        for (int j = 0; j <= s2.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[s1.Length, s2.Length];
    }
}
