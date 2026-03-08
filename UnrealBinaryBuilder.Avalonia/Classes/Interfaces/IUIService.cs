using System;
using System.Threading.Tasks;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

public enum UBBNotificationType { Info, Success, Warning, Error }

public record NotificationEventArgs(string Title, string Message, UBBNotificationType Type);

public enum UBBDialogResult { None, Primary, Secondary }

public interface IUIService
{
    event EventHandler<NotificationEventArgs>? ShowNotification;

    void ShowToast(string message, UBBNotificationType type = UBBNotificationType.Info, string title = "");

    Task<UBBDialogResult> ShowMessageDialog(string title, string content, string primaryButton = "OK", string? secondaryButton = null, string? closeButton = null);

    void ApplyTheme(string theme);

    void OpenUrl(string url);

    void OpenFolder(string path);

    void ShowAboutDialog();

    void OpenCodeEditor(string filePath);

    Task CopyTextToClipboard(string text);

    Task<string?> BrowseFolderAsync(string title);

    Task<string?> BrowseFileAsync(string title, string[] extensions, string filterName);

    Task<string?> SaveFileAsync(string title, string suggestedName, string extension, string filterName);
}
