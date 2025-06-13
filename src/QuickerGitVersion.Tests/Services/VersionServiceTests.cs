using FluentAssertions;
using QuickerGitVersion.Models;
using QuickerGitVersion.Services;
using Xunit;

namespace QuickerGitVersion.Tests.Services;

public class VersionServiceTests
{
    [Fact]
    public void CalculateVersion_Should_ReturnCorrectVersion_ForMainBranch()
    {
        // Arrange
        var versionService = new VersionService();
        var gitInfo = new GitInfo
        {
            LatestTag = "v1.100.1",
            BranchName = "main",
            Sha = "abc123def456",
            ShortSha = "abc123d",
            CommitDate = "2025-06-11",
            CommitsSinceVersionSource = 0,
            UncommittedChanges = 0,
            VersionSourceSha = "abc123def456"
        };

        // Act
        var versionInfo = versionService.CalculateVersion(gitInfo);

        // Assert
        versionInfo.Major.Should().Be(1);
        versionInfo.Minor.Should().Be(100);
        versionInfo.Patch.Should().Be(1);
        versionInfo.BranchName.Should().Be("main");
        versionInfo.SemVer.Should().Be("1.100.1");
        versionInfo.PreReleaseLabel.Should().BeEmpty();
    }

    [Fact]
    public void CalculateVersion_Should_ReturnPreReleaseVersion_ForFeatureBranch()
    {
        // Arrange
        var versionService = new VersionService();
        var gitInfo = new GitInfo
        {
            BranchName = "feature/new-feature",
            Sha = "def456abc789",
            ShortSha = "def456a",
            CommitDate = "2025-06-11",
            CommitsSinceVersionSource = 5,
            UncommittedChanges = 2,
            VersionSourceSha = "abc123def456",
            LatestTag = "1.100.1"
        };

        // Act
        var versionInfo = versionService.CalculateVersion(gitInfo);

        // Assert
        versionInfo.BranchName.Should().Be("feature/new-feature");
        versionInfo.EscapedBranchName.Should().Be("feature-new-feature");
        versionInfo.PreReleaseNumber.Should().Be(5);
        versionInfo.SemVer.Should().Be("1.100.1-feature-new-feature.5");
        versionInfo.PreReleaseLabel.Should().Be("feature-new-feature");
    }

    [Theory]
    [InlineData("feature/test", false)]
    [InlineData("main", true)]
    [InlineData("master", true)]
    [InlineData("Main", true)]  // 大小写不敏感
    [InlineData("develop", false)]
    [InlineData("hotfix/urgent", false)]
    public void IsMainBranch_Should_IdentifyMainBranches_Correctly(string branchName, bool expected)
    {
        // Arrange
        var versionService = new VersionService();
        var gitInfo = new GitInfo { BranchName = branchName };

        // Act
        var versionInfo = versionService.CalculateVersion(gitInfo);
        var isMainBranch = string.IsNullOrEmpty(versionInfo.PreReleaseLabel);

        // Assert
        isMainBranch.Should().Be(expected);
    }

    [Theory]
    [InlineData("feature/new-feature", "feature-new-feature")]
    [InlineData("hotfix\\urgent-fix", "hotfix-urgent-fix")]
    [InlineData("user:name/branch", "user-name-branch")]
    [InlineData("branch*with?special<chars>", "branch-with-special-chars-")]
    public void EscapeBranchName_Should_HandleSpecialCharacters_Correctly(string input, string expected)
    {
        // Arrange
        var versionService = new VersionService();
        var gitInfo = new GitInfo { BranchName = input };

        // Act
        var versionInfo = versionService.CalculateVersion(gitInfo);

        // Assert
        versionInfo.EscapedBranchName.Should().Be(expected);
    }
} 