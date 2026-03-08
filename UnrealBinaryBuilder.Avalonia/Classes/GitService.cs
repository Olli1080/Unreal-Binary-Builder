using System.IO;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class GitService : IGitService
{
    public string GetGitInfo(string enginePath)
    {
        if (string.IsNullOrEmpty(enginePath) || !Directory.Exists(enginePath)) return "Not a git repository.";
        
        try 
        {
            string? branch = GetBranchName(enginePath);
            string? hash = GetCommitHashShort(enginePath);
            
            if (branch != null && hash != null) return $"Branch: {branch} | Hash: {hash}";
            return "Git not detected.";
        } 
        catch 
        { 
            return "Git error."; 
        }
    }

    public string? GetBranchName(string enginePath) => Git.GetBranchName(enginePath);

    public string? GetCommitHashShort(string enginePath) => Git.GetCommitHashShort(enginePath);
}
