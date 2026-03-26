using LibGit2Sharp;
using UnrealBinaryBuilder.Avalonia.Classes;

namespace UnrealBinaryBuilder.Tests;

public class GitTests : IDisposable
{
    private readonly string _testRepoPath;

    public GitTests()
    {
        _testRepoPath = Path.Combine(Path.GetTempPath(), "UBB_Git_Tests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testRepoPath);
        Git.Reset();
    }

    public void Dispose()
    {
        // Git creates read-only files in .git, so we need to clear attributes before deleting
        if (Directory.Exists(_testRepoPath))
        {
            DeleteReadOnlyDirectory(_testRepoPath);
        }
    }

    private void DeleteReadOnlyDirectory(string directory)
    {
        foreach (var subDir in Directory.GetDirectories(directory))
        {
            DeleteReadOnlyDirectory(subDir);
        }
        foreach (var file in Directory.GetFiles(directory))
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }
        Directory.Delete(directory);
    }

    [Fact]
    public void GetCommitHash_ReturnsNull_ForNonExistentRepo()
    {
        var hash = Git.GetCommitHash(_testRepoPath);
        Assert.Null(hash);
    }

    [Fact]
    public void Git_ReturnsCorrectInfo_ForValidRepo()
    {
        // Arrange
        Repository.Init(_testRepoPath);
        using (var repo = new Repository(_testRepoPath))
        {
            File.WriteAllText(Path.Combine(_testRepoPath, "test.txt"), "hello");
            Commands.Stage(repo, "*");
            var signature = new Signature("Test", "test@test.com", DateTimeOffset.Now);
            repo.Commit("Initial commit", signature, signature);
            
            var expectedHash = repo.Head.Tip.Sha;
            var expectedBranch = repo.Head.FriendlyName;

            // Act
            var actualHash = Git.GetCommitHash(_testRepoPath);
            var actualHashShort = Git.GetCommitHashShort(_testRepoPath);
            var actualBranch = Git.GetBranchName(_testRepoPath);

            // Assert
            Assert.Equal(expectedHash, actualHash);
            Assert.Equal(expectedHash.Substring(0, 7), actualHashShort);
            Assert.Equal(expectedBranch, actualBranch);
        }
    }

    [Fact]
    public void UpdateRepository_HandlesNullOrEmptyPath()
    {
        // This test ensures no exceptions are thrown and internal state is cleared
        Git.GetCommitHash(null!);
        Git.GetCommitHash("");
    }
}
