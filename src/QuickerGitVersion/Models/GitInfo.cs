namespace QuickerGitVersion.Models;

public class GitInfo
{
    public string BranchName { get; set; } = string.Empty;
    
    public string Sha { get; set; } = string.Empty;
    
    public string ShortSha { get; set; } = string.Empty;
    
    public string CommitDate { get; set; } = string.Empty;
    
    public int UncommittedChanges { get; set; }
    
    public int CommitsSinceVersionSource { get; set; }
    
    public string VersionSourceSha { get; set; } = string.Empty;
} 