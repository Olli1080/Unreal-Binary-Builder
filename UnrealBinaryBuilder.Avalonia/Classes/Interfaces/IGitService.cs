namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

public interface IGitService
{
    string GetGitInfo(string enginePath);
    string? GetBranchName(string enginePath);
    string? GetCommitHashShort(string enginePath);
}
