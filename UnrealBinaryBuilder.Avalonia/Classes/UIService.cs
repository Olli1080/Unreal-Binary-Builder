using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Views;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class UIService : IUIService
{
    private readonly IPlatformService _platformService;
    private readonly ITelemetryService _telemetry;

    public event EventHandler<NotificationEventArgs>? ShowNotification;

    public UIService(IPlatformService platformService, ITelemetryService telemetry)
    {
        _platformService = platformService;
        _telemetry = telemetry;
    }

    private IStorageProvider? GetStorageProvider() => (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) ? desktop.MainWindow?.StorageProvider : null;

    public void ShowToast(string message, UBBNotificationType type = UBBNotificationType.Info, string title = "")
    {
        ShowNotification?.Invoke(this, new NotificationEventArgs(title, message, type));
    }

    public async Task<UBBDialogResult> ShowMessageDialog(string title, string content, string primaryButton = "OK", string? secondaryButton = null, string? closeButton = null)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = primaryButton,
                SecondaryButtonText = secondaryButton,
                CloseButtonText = closeButton,
                DefaultButton = ContentDialogButton.Primary
            };
            var result = await dialog.ShowAsync();
            return result switch
            {
                ContentDialogResult.Primary => UBBDialogResult.Primary,
                ContentDialogResult.Secondary => UBBDialogResult.Secondary,
                _ => UBBDialogResult.None
            };
        }
        return UBBDialogResult.None;
    }

    public void ApplyTheme(string theme)
    {
        if (Application.Current == null) return;
        
        _telemetry.TrackEvent($"Theme:{theme}");
        
        switch (theme?.ToLower())
        {
            case "light":
                Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                break;
            case "dark":
                Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
                break;
            default:
                Application.Current.RequestedThemeVariant = ThemeVariant.Default;
                break;
        }
    }

    public void OpenUrl(string url)
    {
        _platformService.OpenUrl(url);
    }

    public void OpenFolder(string path)
    {
        _platformService.OpenFolder(path);
    }

    public void ShowAboutDialog()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dlg = new AboutDialog();
            dlg.ShowDialog(desktop.MainWindow!);
        }
    }

    public void OpenCodeEditor(string filePath)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var editor = new CodeEditorWindow();
            editor.LoadFile(filePath);
            editor.Show(desktop.MainWindow!);
        }
    }

    public async Task CopyTextToClipboard(string text)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow?.Clipboard != null)
        {
            await desktop.MainWindow.Clipboard.SetTextAsync(text);
        }
    }

    public async Task<string?> BrowseFolderAsync(string title)
    {
        var sp = GetStorageProvider();
        if (sp == null) return null;
        var res = await sp.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = title, AllowMultiple = false });
        return res.Count > 0 ? res[0].Path.LocalPath : null;
    }

    public async Task<string?> BrowseFileAsync(string title, string[] extensions, string filterName)
    {
        var sp = GetStorageProvider();
        if (sp == null) return null;
        var res = await sp.OpenFilePickerAsync(new FilePickerOpenOptions 
        { 
            Title = title, 
            FileTypeFilter = new[] { new FilePickerFileType(filterName) { Patterns = extensions } },
            AllowMultiple = false 
        });
        return res.Count > 0 ? res[0].Path.LocalPath : null;
    }

    public async Task<string?> SaveFileAsync(string title, string suggestedName, string extension, string filterName)
    {
        var sp = GetStorageProvider();
        if (sp == null) return null;
        var res = await sp.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedName,
            FileTypeChoices = new[] { new FilePickerFileType(filterName) { Patterns = new[] { $"*{extension}" } } },
            DefaultExtension = extension
        });
        return res?.Path.LocalPath;
    }
}
