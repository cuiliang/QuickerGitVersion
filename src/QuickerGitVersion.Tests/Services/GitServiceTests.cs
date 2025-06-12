using FluentAssertions;
using QuickerGitVersion.Services;
using Xunit;
using System.IO;

namespace QuickerGitVersion.Tests.Services;

public class GitServiceTests
{
    [Fact]
    public void FindGitRepositoryRoot_Should_ReturnCurrentDirectory_WhenIsGitRepository()
    {
        // Arrange
        var gitService = new GitService();
        var currentDir = Directory.GetCurrentDirectory();
        
        // 假设测试在git仓库中运行
        if (!Directory.Exists(Path.Combine(currentDir, ".git")))
        {
            // 如果不在git仓库中，跳过此测试
            return;
        }
        
        // Act & Assert
        var exception = Record.Exception(() => gitService.GetGitInfo(currentDir));
        
        // 应该不抛出异常
        exception.Should().BeNull();
        gitService.LastFoundRepositoryPath.Should().NotBeNull();
    }
    
    [Fact]
    public void GetGitInfo_Should_ThrowException_WhenNoGitRepositoryFound()
    {
        // Arrange
        var gitService = new GitService();
        var tempDir = Path.GetTempPath();
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            gitService.GetGitInfo(tempDir));
        
        exception.Message.Should().Contain("未找到Git仓库");
    }
    
    [Fact]
    public void FindGitRepositoryRoot_Should_FindParentGitRepository()
    {
        // Arrange
        var gitService = new GitService();
        var currentDir = Directory.GetCurrentDirectory();
        
        // 查找git仓库根目录
        var gitRoot = FindGitRoot(currentDir);
        if (string.IsNullOrEmpty(gitRoot))
        {
            // 如果不在git仓库中，跳过此测试
            return;
        }
        
        // 创建一个子目录路径进行测试
        var subDir = Path.Combine(gitRoot, "src");
        if (!Directory.Exists(subDir))
        {
            return; // 如果src目录不存在，跳过测试
        }
        
        // Act
        var exception = Record.Exception(() => gitService.GetGitInfo(subDir));
        
        // Assert
        exception.Should().BeNull();
        gitService.LastFoundRepositoryPath.Should().Be(gitRoot);
    }
    
    private string FindGitRoot(string startPath)
    {
        var currentPath = Path.GetFullPath(startPath);
        
        while (currentPath != null)
        {
            if (Directory.Exists(Path.Combine(currentPath, ".git")))
            {
                return currentPath;
            }
            
            var parentPath = Directory.GetParent(currentPath)?.FullName;
            if (parentPath == currentPath)
            {
                break;
            }
            
            currentPath = parentPath;
        }
        
        return string.Empty;
    }
} 