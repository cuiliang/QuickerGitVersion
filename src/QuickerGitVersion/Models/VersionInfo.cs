using System.Text.Json.Serialization;

namespace QuickerGitVersion.Models;

public class VersionInfo
{
    [JsonPropertyName("AssemblySemFileVer")]
    public string AssemblySemFileVer { get; set; } = string.Empty;
    
    [JsonPropertyName("AssemblySemVer")]
    public string AssemblySemVer { get; set; } = string.Empty;
    
    [JsonPropertyName("BranchName")]
    public string BranchName { get; set; } = string.Empty;
    
    [JsonPropertyName("BuildMetaData")]
    public string? BuildMetaData { get; set; }
    
    [JsonPropertyName("CommitDate")]
    public string CommitDate { get; set; } = string.Empty;
    
    [JsonPropertyName("CommitsSinceVersionSource")]
    public int CommitsSinceVersionSource { get; set; }
    
    [JsonPropertyName("EscapedBranchName")]
    public string EscapedBranchName { get; set; } = string.Empty;
    
    [JsonPropertyName("FullBuildMetaData")]
    public string FullBuildMetaData { get; set; } = string.Empty;
    
    [JsonPropertyName("FullSemVer")]
    public string FullSemVer { get; set; } = string.Empty;
    
    [JsonPropertyName("InformationalVersion")]
    public string InformationalVersion { get; set; } = string.Empty;
    
    [JsonPropertyName("Major")]
    public int Major { get; set; }
    
    [JsonPropertyName("MajorMinorPatch")]
    public string MajorMinorPatch { get; set; } = string.Empty;
    
    [JsonPropertyName("Minor")]
    public int Minor { get; set; }
    
    [JsonPropertyName("Patch")]
    public int Patch { get; set; }
    
    [JsonPropertyName("PreReleaseLabel")]
    public string PreReleaseLabel { get; set; } = string.Empty;
    
    [JsonPropertyName("PreReleaseLabelWithDash")]
    public string PreReleaseLabelWithDash { get; set; } = string.Empty;
    
    [JsonPropertyName("PreReleaseNumber")]
    public int PreReleaseNumber { get; set; }
    
    [JsonPropertyName("PreReleaseTag")]
    public string PreReleaseTag { get; set; } = string.Empty;
    
    [JsonPropertyName("PreReleaseTagWithDash")]
    public string PreReleaseTagWithDash { get; set; } = string.Empty;
    
    [JsonPropertyName("SemVer")]
    public string SemVer { get; set; } = string.Empty;
    
    [JsonPropertyName("Sha")]
    public string Sha { get; set; } = string.Empty;
    
    [JsonPropertyName("ShortSha")]
    public string ShortSha { get; set; } = string.Empty;
    
    [JsonPropertyName("UncommittedChanges")]
    public int UncommittedChanges { get; set; }
    
    [JsonPropertyName("VersionSourceSha")]
    public string VersionSourceSha { get; set; } = string.Empty;
    
    [JsonPropertyName("WeightedPreReleaseNumber")]
    public int WeightedPreReleaseNumber { get; set; }
} 