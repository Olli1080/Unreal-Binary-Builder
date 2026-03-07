using LibGit2Sharp;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public static class Git
{
    private static Repository? _repository = null;
    private static string? _currentPath = null;

    public static string? GetCommitHash(string repoPath)
    {
        UpdateRepository(repoPath);
        return _repository?.Head.Tip.Sha;
    }

    public static string? GetCommitHashShort(string repoPath)
    {
        string? hash = GetCommitHash(repoPath);
        return string.IsNullOrWhiteSpace(hash) ? null : hash.Length > 7 ? hash.Remove(7) : hash;
    }

    public static string? GetBranchName(string repoPath)
    {
        UpdateRepository(repoPath);
        return _repository?.Head.FriendlyName;
    }

    public static string? GetTrackedBranchName(string repoPath)
    {
        UpdateRepository(repoPath);
        if (_repository == null) return null;
        
        return _repository.Head.IsTracking ? _repository.Head.TrackedBranch.FriendlyName : null;
    }

    private static void UpdateRepository(string repoPath)
    {
        if (string.IsNullOrWhiteSpace(repoPath))
        {
            _repository?.Dispose();
            _repository = null;
            _currentPath = null;
            return;
        }

        if (_repository == null || _currentPath != repoPath)
        {
            _repository?.Dispose();
            _repository = null;
            _currentPath = repoPath;

            try
            {
                if (Repository.IsValid(repoPath))
                {
                    _repository = new Repository(repoPath);
                }
            }
            catch
            {
                _repository = null;
            }
        }
    }
}
