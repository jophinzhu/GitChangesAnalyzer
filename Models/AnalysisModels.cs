namespace GitChangesAnalyzer.Models;

public class ChangeBlock
{
    public string DiffContent { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string HunkHeader { get; set; } = string.Empty;
    public ChangeType ChangeType { get; set; }
    public LineNumbers LineNumbers { get; set; } = new();
    public List<string> ContextLines { get; set; } = new();
    public string SimilarityHash { get; set; } = string.Empty;
    public int HunkIndex { get; set; }
}

public class ChangeGroup
{
    public string PatternId { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public List<ChangeBlock> ChangeBlocks { get; set; } = new();
    public List<string> AffectedFiles { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public ChangeCategory Category { get; set; }
    public int GroupSize => ChangeBlocks.Count;
}

public class LineNumbers
{
    public int StartOld { get; set; }
    public int EndOld { get; set; }
    public int StartNew { get; set; }
    public int EndNew { get; set; }
}

public class AnalysisMetadata
{
    public DateTime Timestamp { get; set; }
    public int TotalFiles { get; set; }
    public int TotalGroups { get; set; }
    public string GitRange { get; set; } = string.Empty;
    public string RepositoryPath { get; set; } = string.Empty;
    public List<string> CommitHashes { get; set; } = new();
    public int TotalChanges { get; set; }
    public int UniquePatterns { get; set; }
}

public class AnalysisResult
{
    public AnalysisMetadata Metadata { get; set; } = new();
    public List<ChangeGroup> ChangeGroups { get; set; } = new();
    public Dictionary<string, int> CategoryCounts { get; set; } = new();
}

public enum ChangeType
{
    Add,
    Delete,
    Modify,
    Rename,
    Copy
}

public enum ChangeCategory
{
    XmlElement,
    XmlAttribute,
    XmlContent,
    CSharpMethod,
    CSharpProperty,
    CSharpImport,
    ConfigurationChange,
    Documentation,
    Other
}
