using LibGit2Sharp;
using GitChangesAnalyzer.Models;
using System.Text.RegularExpressions;

namespace GitChangesAnalyzer.Core;

public class GitDiffParser
{
    private readonly Repository _repository;
    private readonly bool _verbose;

    public GitDiffParser(Repository repository, bool verbose = false)
    {
        _repository = repository;
        _verbose = verbose;
    }

    public List<ChangeBlock> ParseCommitDiff(string commitHash)
    {
        var commit = _repository.Lookup<Commit>(commitHash);
        if (commit == null)
            throw new ArgumentException($"Commit {commitHash} not found");

        var changes = new List<ChangeBlock>();
        
        // Get the diff between the commit and its parent(s)
        var parent = commit.Parents.FirstOrDefault();
        if (parent == null)
        {
            // This is the initial commit, compare against empty tree
            var emptyTree = _repository.ObjectDatabase.CreateTree(new TreeDefinition());
            var patch = _repository.Diff.Compare<Patch>(emptyTree, commit.Tree);
            changes.AddRange(ParsePatchEntries(patch));
        }
        else
        {
            var patch = _repository.Diff.Compare<Patch>(parent.Tree, commit.Tree);
            changes.AddRange(ParsePatchEntries(patch));
        }

        if (_verbose)
            Console.WriteLine($"Parsed {changes.Count} change blocks from commit {commitHash}");

        return changes;
    }

    public List<ChangeBlock> ParseCommitRangeDiff(string startCommit, string endCommit)
    {
        var start = _repository.Lookup<Commit>(startCommit);
        var end = _repository.Lookup<Commit>(endCommit);
        
        if (start == null || end == null)
            throw new ArgumentException("One or both commits not found");

        var patch = _repository.Diff.Compare<Patch>(start.Tree, end.Tree);
        var changes = ParsePatchEntries(patch);

        if (_verbose)
            Console.WriteLine($"Parsed {changes.Count} change blocks from range {startCommit}..{endCommit}");

        return changes;
    }

    private List<ChangeBlock> ParsePatchEntries(Patch patch)
    {
        var changes = new List<ChangeBlock>();

        foreach (var patchEntry in patch)
        {
            var changeType = GetChangeType(patchEntry.Status);
            var filePath = patchEntry.Path;

            var hunks = ParseHunks(patchEntry.Patch, filePath, changeType);
            changes.AddRange(hunks);
        }

        return changes;
    }

    private List<ChangeBlock> ParseHunks(string patchContent, string filePath, ChangeType changeType)
    {
        var changes = new List<ChangeBlock>();
        
        if (string.IsNullOrEmpty(patchContent))
            return changes;

        var lines = patchContent.Split('\n');
        var currentHunk = new List<string>();
        var hunkIndex = 0;
        string? currentHunkHeader = null;

        foreach (var line in lines)
        {
            if (line.StartsWith("@@"))
            {
                // Process previous hunk if exists
                if (currentHunk.Count > 0 && currentHunkHeader != null)
                {
                    var changeBlock = CreateChangeBlock(currentHunk, filePath, changeType, currentHunkHeader, hunkIndex++);
                    changes.Add(changeBlock);
                    currentHunk.Clear();
                }

                currentHunkHeader = line;
                currentHunk.Add(line);
            }
            else if (currentHunkHeader != null)
            {
                currentHunk.Add(line);
            }
        }

        // Process the last hunk
        if (currentHunk.Count > 0 && currentHunkHeader != null)
        {
            var changeBlock = CreateChangeBlock(currentHunk, filePath, changeType, currentHunkHeader, hunkIndex);
            changes.Add(changeBlock);
        }

        return changes;
    }

    private ChangeBlock CreateChangeBlock(List<string> hunkLines, string filePath, ChangeType changeType, string hunkHeader, int hunkIndex)
    {
        var changeBlock = new ChangeBlock
        {
            FilePath = filePath,
            ChangeType = changeType,
            HunkHeader = hunkHeader,
            HunkIndex = hunkIndex,
            DiffContent = string.Join("\n", hunkLines),
            LineNumbers = ParseLineNumbers(hunkHeader),
            ContextLines = ExtractContextLines(hunkLines)
        };

        // Generate similarity hash
        changeBlock.SimilarityHash = GenerateSimilarityHash(hunkLines);

        return changeBlock;
    }

    private LineNumbers ParseLineNumbers(string hunkHeader)
    {
        // Parse hunk header like "@@ -15,6 +15,8 @@ namespace MyApp"
        var match = Regex.Match(hunkHeader, @"@@ -(\d+),?(\d*) \+(\d+),?(\d*) @@");
        
        if (match.Success)
        {
            var startOld = int.Parse(match.Groups[1].Value);
            var countOld = string.IsNullOrEmpty(match.Groups[2].Value) ? 1 : int.Parse(match.Groups[2].Value);
            var startNew = int.Parse(match.Groups[3].Value);
            var countNew = string.IsNullOrEmpty(match.Groups[4].Value) ? 1 : int.Parse(match.Groups[4].Value);

            return new LineNumbers
            {
                StartOld = startOld,
                EndOld = startOld + countOld - 1,
                StartNew = startNew,
                EndNew = startNew + countNew - 1
            };
        }

        return new LineNumbers();
    }

    private List<string> ExtractContextLines(List<string> hunkLines)
    {
        return hunkLines
            .Where(line => !line.StartsWith("@@") && !string.IsNullOrEmpty(line))
            .Take(5) // First 5 lines for context
            .ToList();
    }    private string GenerateSimilarityHash(List<string> hunkLines)
    {
        // Create a normalized version for similarity comparison using ONLY changed lines
        var changedLines = hunkLines
            .Where(line => line.StartsWith("+") || line.StartsWith("-"))
            .Where(line => !line.StartsWith("+++") && !line.StartsWith("---"))
            .Select(line => line.Substring(1).Trim()) // Remove +/- marker and normalize whitespace
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var normalizedContent = string.Join("|", changedLines);
        return normalizedContent.GetHashCode().ToString();
    }

    private static ChangeType GetChangeType(ChangeKind status)
    {
        return status switch
        {
            ChangeKind.Added => ChangeType.Add,
            ChangeKind.Deleted => ChangeType.Delete,
            ChangeKind.Modified => ChangeType.Modify,
            ChangeKind.Renamed => ChangeType.Rename,
            ChangeKind.Copied => ChangeType.Copy,
            _ => ChangeType.Modify
        };
    }
}
