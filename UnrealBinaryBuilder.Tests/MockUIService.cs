using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

namespace UnrealBinaryBuilder.Tests;

public class MockUIService : IUIService
{
    public event EventHandler<NotificationEventArgs>? ShowNotification;

    public void ShowToast(string message, UBBNotificationType type = UBBNotificationType.Info, string title = "")
    {
        ShowNotification?.Invoke(this, new NotificationEventArgs(title, message, type));
    }

    public Task<UBBDialogResult> ShowMessageDialog(string title, string content, string primaryButton = "OK", string? secondaryButton = null, string? closeButton = null)
    {
        return Task.FromResult(UBBDialogResult.None);
    }

    public void ApplyTheme(string theme) { }

    public void OpenUrl(string url) { }

    public void OpenFolder(string path) { }

    public void ShowAboutDialog() { }

    public void OpenCodeEditor(string filePath) { }

    public Task CopyTextToClipboard(string text) => Task.CompletedTask;

    public Task<string?> BrowseFolderAsync(string title) => Task.FromResult<string?>(null);

    public Task<string?> BrowseFileAsync(string title, string[] extensions, string filterName) => Task.FromResult<string?>(null);

    public Task<string?> SaveFileAsync(string title, string suggestedName, string extension, string filterName) => Task.FromResult<string?>(null);
}
