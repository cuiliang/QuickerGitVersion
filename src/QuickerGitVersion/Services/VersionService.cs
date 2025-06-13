using QuickerGitVersion.Models;
using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace QuickerGitVersion.Services;

public class VersionService
{
    public VersionInfo CalculateVersion(GitInfo gitInfo)
    {
        var versionInfo = new VersionInfo();
        
        // 1. 解析基础版本
        ParseBaseVersion(gitInfo, versionInfo);
        
        // 2. 设置基础属性
        versionInfo.BranchName = gitInfo.BranchName;
        versionInfo.EscapedBranchName = EscapeBranchName(gitInfo.BranchName);
        versionInfo.Sha = gitInfo.Sha;
        versionInfo.ShortSha = gitInfo.ShortSha;
        versionInfo.CommitDate = gitInfo.CommitDate;
        versionInfo.CommitsSinceVersionSource = gitInfo.CommitsSinceVersionSource;
        versionInfo.UncommittedChanges = gitInfo.UncommittedChanges;
        versionInfo.VersionSourceSha = gitInfo.VersionSourceSha;
        
        // 3. 计算版本字符串
        versionInfo.MajorMinorPatch = $"{versionInfo.Major}.{versionInfo.Minor}.{versionInfo.Patch}";
        versionInfo.AssemblySemVer = $"{versionInfo.MajorMinorPatch}.{gitInfo.CommitsSinceVersionSource}";
        versionInfo.AssemblySemFileVer = versionInfo.AssemblySemVer;
        
        // 4. 预发布版本处理
        if (!IsMainBranch(gitInfo.BranchName))
        {
            versionInfo.PreReleaseLabel = versionInfo.EscapedBranchName;
            versionInfo.PreReleaseLabelWithDash = $"-{versionInfo.EscapedBranchName}";
            versionInfo.PreReleaseNumber = gitInfo.CommitsSinceVersionSource;
            versionInfo.WeightedPreReleaseNumber = versionInfo.PreReleaseNumber;
            versionInfo.PreReleaseTag = $"{versionInfo.EscapedBranchName}.{versionInfo.PreReleaseNumber}";
            versionInfo.PreReleaseTagWithDash = $"-{versionInfo.EscapedBranchName}.{versionInfo.PreReleaseNumber}";
            
            versionInfo.SemVer = $"{versionInfo.MajorMinorPatch}{versionInfo.PreReleaseTagWithDash}";
            versionInfo.FullSemVer = versionInfo.SemVer;
        }
        else
        {
            versionInfo.SemVer = versionInfo.MajorMinorPatch;
            versionInfo.FullSemVer = versionInfo.MajorMinorPatch;
            versionInfo.PreReleaseLabel = string.Empty;
            versionInfo.PreReleaseLabelWithDash = string.Empty;
            versionInfo.PreReleaseTag = string.Empty;
            versionInfo.PreReleaseTagWithDash = string.Empty;
        }
        
        // 5. 构建元数据
        var buildMetadata = $"Branch.{versionInfo.EscapedBranchName}.Sha.{versionInfo.Sha}";
        if (gitInfo.UncommittedChanges > 0)
        {
            buildMetadata += $".UncommittedChanges.{gitInfo.UncommittedChanges}";
        }
        versionInfo.FullBuildMetaData = buildMetadata;
        versionInfo.InformationalVersion = $"{versionInfo.FullSemVer}+{buildMetadata}";
        
        return versionInfo;
    }
    
    private void ParseBaseVersion(GitInfo gitInfo, VersionInfo versionInfo)
    {
        if (!string.IsNullOrEmpty(gitInfo.LatestTag))
        {
            try
            {
                var versionString = gitInfo.LatestTag.TrimStart('v');
                var version = new Version(versionString);
                versionInfo.Major = version.Major;
                versionInfo.Minor = version.Minor;
                versionInfo.Patch = version.Build >= 0 ? version.Build : 0;
            }
            catch
            {
                // 如果标签解析失败，使用默认版本号
                SetDefaultVersion(versionInfo);
            }
        }
        else
        {
            // 如果没有找到版本标签，使用默认版本号
            SetDefaultVersion(versionInfo);
        }
    }

    private void SetDefaultVersion(VersionInfo versionInfo)
    {
        versionInfo.Major = 0;
        versionInfo.Minor = 0;
        versionInfo.Patch = 0;
    }

    private string EscapeBranchName(string branchName)
    {
        if (string.IsNullOrEmpty(branchName))
        {
            return "unknown";
        }
        
        return Regex.Replace(branchName, @"[^a-zA-Z0-9-]", "-");
    }
    
    private bool IsMainBranch(string branchName)
    {
        // 只认 main/master，忽略其它
        var cleanBranchName = branchName;
        if (branchName.StartsWith("remotes/"))
        {
            var parts = branchName.Split('/');
            if (parts.Length >= 3)
            {
                cleanBranchName = string.Join("/", parts.Skip(2));
            }
        }
        return string.Equals(cleanBranchName, "main", StringComparison.OrdinalIgnoreCase)
            || string.Equals(cleanBranchName, "master", StringComparison.OrdinalIgnoreCase);
    }
} 