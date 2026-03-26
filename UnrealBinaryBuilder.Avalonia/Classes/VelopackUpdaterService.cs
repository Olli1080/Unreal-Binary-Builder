using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;
using UnrealBinaryBuilder.Avalonia.Classes.Logging;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public interface IVelopackUpdaterService
{
    bool IsUpdateAvailable { get; }
    Task CheckForUpdatesAsync(bool silent = false);
    Task DownloadUpdatesAsync();
    void ApplyUpdatesAndRestart();
}

public class VelopackUpdaterService : IVelopackUpdaterService
{
    private readonly IUBBLogger _logger;
    private readonly string _repoUrl = "https://github.com/Olli1080/Unreal-Binary-Builder";
    private UpdateInfo? _updateInfo;

    public bool IsUpdateAvailable => _updateInfo != null;

    public VelopackUpdaterService(IUBBLogger logger)
    {
        _logger = logger;
    }

    public async Task CheckForUpdatesAsync(bool silent = false)
    {
        try
        {
            var mgr = new UpdateManager(new GithubSource(_repoUrl, null, false));
            _updateInfo = await mgr.CheckForUpdatesAsync();

            if (_updateInfo != null)
            {
                _logger.Info($"Update available: {_updateInfo.TargetFullRelease.Version}", LogCategory.General);
            }
            else if (!silent)
            {
                _logger.Info("No updates available.", LogCategory.General);
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to check for updates: {ex.Message}", LogCategory.General);
        }
    }

    public async Task DownloadUpdatesAsync()
    {
        if (_updateInfo == null) return;

        try
        {
            var mgr = new UpdateManager(new GithubSource(_repoUrl, null, false));
            _logger.Info("Downloading update...", LogCategory.General);
            await mgr.DownloadUpdatesAsync(_updateInfo);
            _logger.Info("Update downloaded and ready to install.", LogCategory.General);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to download update: {ex.Message}", LogCategory.General);
        }
    }

    public void ApplyUpdatesAndRestart()
    {
        if (_updateInfo == null) return;

        try
        {
            var mgr = new UpdateManager(new GithubSource(_repoUrl, null, false));
            mgr.ApplyUpdatesAndRestart(_updateInfo);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to apply update: {ex.Message}", LogCategory.General);
        }
    }
}
