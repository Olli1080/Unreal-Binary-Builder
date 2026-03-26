using System.IO;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

namespace UnrealBinaryBuilder.Avalonia.Classes;

/// <summary>
/// Provides services for interacting with Git repositories.
/// </summary>
public class GitService : IGitService
{
    /// <summary>
    /// Retrieves a formatted string containing the current Git branch and commit hash for the specified repository.
    /// </summary>
    /// <param name="enginePath">The path to the Git repository (e.g., the Unreal Engine directory).</param>
    /// <returns>A string detailing the branch and commit hash, or an error message if Git info could not be retrieved.</returns>
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

    /// <summary>
    /// Retrieves the current Git branch name for the specified repository.
    /// </summary>
    /// <param name="enginePath">The path to the Git repository.</param>
    /// <returns>The branch name, or null if it cannot be determined.</returns>
    public string? GetBranchName(string enginePath) => Git.GetBranchName(enginePath);

    /// <summary>
    /// Retrieves the short Git commit hash for the specified repository.
    /// </summary>
    /// <param name="enginePath">The path to the Git repository.</param>
    /// <returns>The short commit hash, or null if it cannot be determined.</returns>
    public string? GetCommitHashShort(string enginePath) => Git.GetCommitHashShort(enginePath);
}
