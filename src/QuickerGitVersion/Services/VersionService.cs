using QuickerGitVersion.Models;

namespace QuickerGitVersion.Services;

public class VersionService
{
    public VersionInfo CalculateVersion(GitInfo gitInfo)
    {
        var versionInfo = new VersionInfo();
        
        // 解析基础版本号（简化版本，实际应从Git标签解析）
        versionInfo.Major = 1;
        versionInfo.Minor = 100;
        versionInfo.Patch = 1;
        
        // 设置基础属性
        versionInfo.BranchName = gitInfo.BranchName;
        versionInfo.EscapedBranchName = EscapeBranchName(gitInfo.BranchName);
        versionInfo.Sha = gitInfo.Sha;
        versionInfo.ShortSha = gitInfo.ShortSha;
        versionInfo.CommitDate = gitInfo.CommitDate;
        versionInfo.CommitsSinceVersionSource = gitInfo.CommitsSinceVersionSource;
        versionInfo.UncommittedChanges = gitInfo.UncommittedChanges;
        versionInfo.VersionSourceSha = gitInfo.VersionSourceSha;
        
        // 计算版本字符串
        versionInfo.MajorMinorPatch = $"{versionInfo.Major}.{versionInfo.Minor}.{versionInfo.Patch}";
        versionInfo.AssemblySemVer = $"{versionInfo.MajorMinorPatch}.0";
        versionInfo.AssemblySemFileVer = versionInfo.AssemblySemVer;
        
        // 预发布版本处理
        if (!IsMainBranch(gitInfo.BranchName))
        {
            versionInfo.PreReleaseLabel = "{BranchName}";
            versionInfo.PreReleaseLabelWithDash = "-{BranchName}";
            versionInfo.PreReleaseNumber = gitInfo.CommitsSinceVersionSource;
            versionInfo.WeightedPreReleaseNumber = versionInfo.PreReleaseNumber;
            versionInfo.PreReleaseTag = $"{{BranchName}}.{versionInfo.PreReleaseNumber}";
            versionInfo.PreReleaseTagWithDash = $"-{{BranchName}}.{versionInfo.PreReleaseNumber}";
            
            versionInfo.SemVer = $"{versionInfo.MajorMinorPatch}-{{BranchName}}.{versionInfo.PreReleaseNumber}";
            versionInfo.FullSemVer = versionInfo.SemVer;
        }
        else
        {
            versionInfo.SemVer = versionInfo.MajorMinorPatch;
            versionInfo.FullSemVer = versionInfo.SemVer;
            versionInfo.PreReleaseLabel = string.Empty;
            versionInfo.PreReleaseLabelWithDash = string.Empty;
            versionInfo.PreReleaseTag = string.Empty;
            versionInfo.PreReleaseTagWithDash = string.Empty;
        }
        
        // 构建元数据
        versionInfo.FullBuildMetaData = $"Branch.{versionInfo.EscapedBranchName}.Sha.{versionInfo.Sha}";
        versionInfo.InformationalVersion = $"{versionInfo.FullSemVer}+{versionInfo.FullBuildMetaData}";
        
        return versionInfo;
    }
    
    private string EscapeBranchName(string branchName)
    {
        if (string.IsNullOrEmpty(branchName))
        {
            return "unknown";
        }
        
        // 处理超长分支名
        const int maxLength = 50;
        if (branchName.Length > maxLength)
        {
            branchName = branchName.Substring(0, maxLength - 3) + "...";
        }
        
        // 转义特殊字符
        return branchName
            .Replace("/", "-")
            .Replace("\\", "-")
            .Replace(":", "-")
            .Replace("*", "-")
            .Replace("?", "-")
            .Replace("\"", "-")
            .Replace("<", "-")
            .Replace(">", "-")
            .Replace("|", "-");
    }
    
    private bool IsMainBranch(string branchName)
    {
        return branchName.Equals("main", StringComparison.OrdinalIgnoreCase) ||
               branchName.Equals("master", StringComparison.OrdinalIgnoreCase);
    }
} 