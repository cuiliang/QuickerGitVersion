using LibGit2Sharp;
using QuickerGitVersion.Models;
using System.Text.RegularExpressions;

namespace QuickerGitVersion.Services;

public class GitService
{
    public string? LastFoundRepositoryPath { get; private set; }
    
    public GitInfo GetGitInfo(string repositoryPath)
    {
        try
        {
            // 查找Git仓库根目录
            var gitRepoPath = FindGitRepositoryRoot(repositoryPath);
            if (string.IsNullOrEmpty(gitRepoPath))
            {
                throw new InvalidOperationException("当前目录或其父目录中未找到Git仓库");
            }
            
            LastFoundRepositoryPath = gitRepoPath;
            using var repo = new Repository(gitRepoPath);
            
            // 检查仓库是否为空
            if (repo.Head?.Tip == null)
            {
                return new GitInfo
                {
                    BranchName = "HEAD",
                    Sha = string.Empty,
                    ShortSha = string.Empty,
                    CommitDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    UncommittedChanges = 0,
                    CommitsSinceVersionSource = 0,
                    VersionSourceSha = string.Empty
                };
            }
            
            // 获取当前分支名
            var branchName = GetBranchName(repo);
            
            // 获取当前提交
            var currentCommit = repo.Head.Tip;
            
            // 获取SHA值
            var sha = currentCommit.Sha;
            var shortSha = sha.Substring(0, 7);
            
            // 计算未提交更改数量
            var status = repo.RetrieveStatus();
            var uncommittedChanges = status.Count();
            
            // 获取提交日期
            var commitDate = currentCommit.Author.When.ToString("yyyy-MM-dd");
            
            // 查找版本标签
            var versionTags = repo.Tags
                .Select(t =>
                {
                    var friendlyName = t.FriendlyName;
                    var success = System.Version.TryParse(friendlyName.TrimStart('v'), out var version);
                    return new { Tag = t, Version = version, Success = success, IsVersion = IsVersionTag(friendlyName) };
                })
                .Where(x => x.Success && x.IsVersion)
                .OrderByDescending(x => x.Version)
                .Select(x => x.Tag)
                .ToList();

            var latestVersionTag = versionTags.FirstOrDefault();
            
            // 计算自版本源以来的提交数
            var commitsSinceVersion = CalculateCommitsSinceVersion(repo, latestVersionTag);
            
            return new GitInfo
            {
                BranchName = branchName,
                Sha = sha,
                ShortSha = shortSha,
                CommitDate = commitDate,
                UncommittedChanges = uncommittedChanges,
                CommitsSinceVersionSource = commitsSinceVersion,
                VersionSourceSha = latestVersionTag?.Target.Sha ?? currentCommit.Sha,
                LatestTag = latestVersionTag?.FriendlyName
            };
        }
        catch (RepositoryNotFoundException ex)
        {
            throw new InvalidOperationException("未找到Git仓库，请在Git仓库目录中运行此命令", ex);
        }
        catch (LibGit2SharpException ex)
        {
            throw new InvalidOperationException($"Git操作失败: {ex.Message}", ex);
        }
    }
    
    private string FindGitRepositoryRoot(string startPath)
    {
        var currentPath = Path.GetFullPath(startPath);
        
        while (currentPath != null)
        {
            if (Repository.IsValid(currentPath))
            {
                return currentPath;
            }
            
            // 检查是否到达根目录
            var parentPath = Directory.GetParent(currentPath)?.FullName;
            if (parentPath == currentPath)
            {
                break;
            }
            
            currentPath = parentPath;
        }
        
        return string.Empty;
    }
    
    private string GetBranchName(Repository repo)
    {
        if (repo.Head.IsCurrentRepositoryHead)
        {
            var friendlyName = repo.Head.FriendlyName;
            // 检查是否为分离头指针状态
            if (friendlyName == "(no branch)")
            {
                try
                {
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = "name-rev --name-only HEAD",
                        WorkingDirectory = repo.Info.WorkingDirectory,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = System.Diagnostics.Process.Start(startInfo);
                    if (process != null)
                    {
                        var output = process.StandardOutput.ReadToEnd().Trim();
                        process.WaitForExit();

                        if (!string.IsNullOrEmpty(output))
                        {
                            // 从输出中提取分支名，例如从 "remotes/origin/v2/try1" 提取 "v2/try1"
                            var parts = output.Split('/');
                            if (parts.Length >= 2)
                            {
                                // 跳过 "remotes" 和 "origin"，取剩余部分
                                return string.Join("/", parts.Skip(2));
                            }
                            return output;
                        }
                    }
                }
                catch
                {
                    // 如果 git name-rev 命令失败，回退到使用 SHA
                }

                // 如果上述方法都失败，使用提交SHA作为标识
                return $"HEAD-{repo.Head.Tip.Sha.Substring(0, 8)}";
            }
            return friendlyName;
        }
        else
        {
            // 非当前仓库头指针状态，使用提交SHA作为标识
            return $"HEAD-{repo.Head.Tip.Sha.Substring(0, 8)}";
        }
    }
    
    private bool IsVersionTag(string tagName)
    {
        return Regex.IsMatch(tagName, @"^v?\d+\.\d+\.\d+");
    }
    
    private DateTimeOffset GetCommitDate(GitObject target)
    {
        return target switch
        {
            Commit commit => commit.Committer.When,
            TagAnnotation tag => tag.Tagger.When,
            _ => DateTimeOffset.MinValue
        };
    }
    
    private int CalculateCommitsSinceVersion(Repository repo, Tag? versionTag)
    {
        if (repo.Head?.Tip == null) return 0;
        
        var headCommit = repo.Head.Tip;

        if (versionTag == null)
        {
            try
            {
                return repo.Commits.QueryBy(new CommitFilter { IncludeReachableFrom = headCommit }).Count();
            }
            catch
            {
                return 1; 
            }
        }

        if (headCommit.Sha == versionTag.Target.Sha)
        {
            return 0;
        }
        
        try
        {
            var filter = new CommitFilter
            {
                IncludeReachableFrom = headCommit,
                ExcludeReachableFrom = versionTag.Target
            };
            
            return repo.Commits.QueryBy(filter).Count();
        }
        catch
        {
            return repo.Commits.QueryBy(new CommitFilter { IncludeReachableFrom = headCommit }).Count();
        }
    }
} 