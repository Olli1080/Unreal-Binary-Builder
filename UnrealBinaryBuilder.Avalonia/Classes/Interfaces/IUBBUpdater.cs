using System;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

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
    public dynamic? CastItem { get; set; }
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
    public object? CastItem { get; set; }
    public string? UpdateFilePath { get; set; }
}

public interface IUBBUpdater
{
    event EventHandler<UpdateProgressFinishedEventArgs>? SilentUpdateFinishedEventHandler;
    event EventHandler<UpdateProgressDownloadEventArgs>? UpdateProgressEventHandler;
    event EventHandler<UpdateProgressDownloadErrorEventArgs>? UpdateProgressDownloadErrorEventHandler;
    event EventHandler<UpdateProgressDownloadStartEventArgs>? UpdateDownloadStartedEventHandler;
    event EventHandler<UpdateProgressDownloadFinishEventArgs>? UpdateDownloadFinishedEventHandler;

    void CheckForUpdates();
    void CheckForUpdatesSilently();
    void DownloadUpdate();
}
