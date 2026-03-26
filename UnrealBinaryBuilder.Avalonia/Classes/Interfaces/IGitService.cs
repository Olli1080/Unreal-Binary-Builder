namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

/// <summary>
/// Provides a service for retrieving Git repository information for a specified path.
/// </summary>
public interface IGitService
{
    /// <summary>
    /// Retrieves general Git information for the specified engine path.
    /// </summary>
    /// <param name="enginePath">The path to the Unreal Engine source repository.</param>
    /// <returns>A string containing Git branch and commit information.</returns>
    string GetGitInfo(string enginePath);

    /// <summary>
    /// Gets the name of the current Git branch for the specified engine path.
    /// </summary>
    /// <param name="enginePath">The path to the Unreal Engine source repository.</param>
    /// <returns>The name of the branch, or null if it cannot be determined.</returns>
    string? GetBranchName(string enginePath);

    /// <summary>
    /// Gets the short commit hash of the current HEAD for the specified engine path.
    /// </summary>
    /// <param name="enginePath">The path to the Unreal Engine source repository.</param>
    /// <returns>The short commit hash, or null if it cannot be determined.</returns>
    string? GetCommitHashShort(string enginePath);
}
