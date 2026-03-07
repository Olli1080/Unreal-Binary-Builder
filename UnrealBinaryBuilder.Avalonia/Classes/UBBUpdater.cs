using NetSparkleUpdater;
using NetSparkleUpdater.SignatureVerifiers;
using System;
using NetSparkleUpdater.Events;
using System.Linq;
using NetSparkleUpdater.Enums;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public enum AppUpdateCheckStatus
{
    UpdateAvailable,
    NoUpdate,
    UserSkip,
    CouldNotDetermine
}

public class UpdateProgressFinishedEventArgs : EventArgs
{
    public AppUpdateCheckStatus AppUpdateCheckStatus { get; set; }
    public AppCastItem? CastItem { get; set; }
}

public class UpdateProgressDownloadEventArgs : EventArgs
{
    public int AppUpdateProgress { get; set; }
}

public class UpdateProgressDownloadErrorEventArgs : EventArgs
{
    public Exception? ErrorException { get; set; }
}

public class UpdateProgressDownloadStartEventArgs : EventArgs
{
    public long UpdateSize { get; set; }
    public string? Version { get; set; }
}

public class UpdateProgressDownloadFinishEventArgs : EventArgs
{
    public AppCastItem? CastItem { get; set; }
    public string? UpdateFilePath { get; set; }
}

public class UBBUpdater
{
    private static readonly string APP_CAST_XML = "https://github.com/ryanjon2040/Unreal-Binary-Builder/raw/master/UnrealBinaryBuilderUpdater/appcast.xml";
    private UpdateInfo? _updateInfo;
    private SparkleUpdater? _sparkle;
    private string? _downloadPath;

    public event EventHandler<UpdateProgressFinishedEventArgs>? SilentUpdateFinishedEventHandler;
    public event EventHandler<UpdateProgressDownloadEventArgs>? UpdateProgressEventHandler;
    public event EventHandler<UpdateProgressDownloadErrorEventArgs>? UpdateProgressDownloadErrorEventHandler;
    public event EventHandler<UpdateProgressDownloadStartEventArgs>? UpdateDownloadStartedEventHandler;
    public event EventHandler<UpdateProgressDownloadFinishEventArgs>? UpdateDownloadFinishedEventHandler;

    public UBBUpdater()
    {
        Internal_SetupSparkle();
    }

    private void Internal_SetupSparkle()
    {
        if (_sparkle == null)
        {
            _sparkle = new SparkleUpdater(APP_CAST_XML, new DSAChecker(SecurityMode.UseIfPossible, "+mLdLTe3Mj6OU0Kr6+ZDeVj+TTFRsNUJvUaPhuJ7pUI="));
            // In Avalonia, we should use the Avalonia UIFactory
            _sparkle.UIFactory = new NetSparkleUpdater.UI.Avalonia.UIFactory();
        }
    }

    public void CheckForUpdates()
    {
        Internal_SetupSparkle();
        _sparkle?.CheckForUpdatesAtUserRequest();
    }

    public async void CheckForUpdatesSilently()
    {
        if (_sparkle == null) return;

        var oldUIFactory = _sparkle.UIFactory;
        _sparkle.UIFactory = null;
        _updateInfo = await _sparkle.CheckForUpdatesQuietly();
        _sparkle.UIFactory = oldUIFactory;

        if (_updateInfo != null)
        {
            UpdateProgressFinishedEventArgs eventArgs = new UpdateProgressFinishedEventArgs();
            eventArgs.CastItem = null;
            switch (_updateInfo.Status)
            {
                case UpdateStatus.UpdateAvailable:
                    eventArgs.AppUpdateCheckStatus = AppUpdateCheckStatus.UpdateAvailable;
                    eventArgs.CastItem = _updateInfo.Updates.First();
                    break;
                case UpdateStatus.UpdateNotAvailable:
                    eventArgs.AppUpdateCheckStatus = AppUpdateCheckStatus.NoUpdate;
                    break;
                case UpdateStatus.UserSkipped:
                    eventArgs.AppUpdateCheckStatus = AppUpdateCheckStatus.UserSkip;
                    break;
                case UpdateStatus.CouldNotDetermine:
                    eventArgs.AppUpdateCheckStatus = AppUpdateCheckStatus.CouldNotDetermine;
                    break;
            }

            SilentUpdateFinishedEventHandler?.Invoke(this, eventArgs);
        }
    }

    public async void DownloadUpdate()
    {
        if (_sparkle == null || _updateInfo?.Updates == null || !_updateInfo.Updates.Any()) return;

        _sparkle.DownloadStarted -= OnDownloadStart;
        _sparkle.DownloadStarted += OnDownloadStart;

        _sparkle.DownloadFinished -= OnDownloadFinish;
        _sparkle.DownloadFinished += OnDownloadFinish;

        _sparkle.DownloadHadError -= OnDownloadError;
        _sparkle.DownloadHadError += OnDownloadError;

        _sparkle.DownloadMadeProgress += OnDownloadMadeProgress;

        await _sparkle.InitAndBeginDownload(_updateInfo.Updates.First());
    }

    private void OnDownloadStart(AppCastItem item, string path)
    {
        UpdateDownloadStartedEventHandler?.Invoke(this, new UpdateProgressDownloadStartEventArgs 
        { 
            UpdateSize = item.UpdateSize, 
            Version = item.Version 
        });
    }

    private void OnDownloadFinish(AppCastItem item, string path)
    {
        _downloadPath = path;
        if (System.IO.File.Exists(_downloadPath + ".zip"))
        {
            System.IO.File.Delete(_downloadPath + ".zip");
        }

        System.IO.File.Move(_downloadPath, _downloadPath + ".zip");
        string newFile = _downloadPath + ".zip";
        
        UpdateDownloadFinishedEventHandler?.Invoke(this, new UpdateProgressDownloadFinishEventArgs 
        { 
            CastItem = item, 
            UpdateFilePath = newFile 
        });
    }

    private void OnDownloadMadeProgress(object? sender, AppCastItem appCastItem, ItemDownloadProgressEventArgs e)
    {
        UpdateProgressEventHandler?.Invoke(this, new UpdateProgressDownloadEventArgs 
        { 
            AppUpdateProgress = e.ProgressPercentage 
        });
    }

    private void OnDownloadError(AppCastItem item, string? path, Exception ex)
    {
        UpdateProgressDownloadErrorEventHandler?.Invoke(this, new UpdateProgressDownloadErrorEventArgs 
        { 
            ErrorException = ex 
        });
    }
}
