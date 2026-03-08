using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

namespace UnrealBinaryBuilder.Tests;

public class MockGitService : IGitService
{
    public string GitInfo { get; set; } = "Mock Git Info";
    public string? BranchName { get; set; } = "mock-branch";
    public string? CommitHash { get; set; } = "mock-hash";

    public string GetGitInfo(string enginePath) => GitInfo;
    public string? GetBranchName(string enginePath) => BranchName;
    public string? GetCommitHashShort(string enginePath) => CommitHash;
}
