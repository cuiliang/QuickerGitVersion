using LibGit2Sharp;
using QuickerGitVersion.Models;
using System.Text.RegularExpressions;

namespace QuickerGitVersion.Services;

public class GitService
{
    public GitInfo GetGitInfo(string repositoryPath)
    {
        try
        {
            // 检查目录是否包含.git文件夹
            if (!Repository.IsValid(repositoryPath))
            {
                throw new InvalidOperationException("当前目录不是有效的Git仓库");
            }
            
            using var repo = new Repository(repositoryPath);
            
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
            var tags = repo.Tags
                .Where(t => IsVersionTag(t.FriendlyName))
                .OrderByDescending(t => GetCommitDate(t.Target))
                .ToList();
            
            // 计算自版本源以来的提交数
            var commitsSinceVersion = CalculateCommitsSinceVersion(repo, tags.FirstOrDefault());
            
            return new GitInfo
            {
                BranchName = branchName,
                Sha = sha,
                ShortSha = shortSha,
                CommitDate = commitDate,
                UncommittedChanges = uncommittedChanges,
                CommitsSinceVersionSource = commitsSinceVersion,
                VersionSourceSha = tags.FirstOrDefault()?.Target.Sha ?? string.Empty
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
    
    private string GetBranchName(Repository repo)
    {
        if (repo.Head.IsCurrentRepositoryHead)
        {
            // 正常分支
            return repo.Head.FriendlyName;
        }
        else
        {
            // 分离HEAD状态，使用提交SHA作为标识
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
        if (versionTag == null) return 0;
        
        try
        {
            var filter = new CommitFilter
            {
                IncludeReachableFrom = repo.Head,
                ExcludeReachableFrom = versionTag.Target
            };
            
            return repo.Commits.QueryBy(filter).Count();
        }
        catch
        {
            return 0;
        }
    }
} 