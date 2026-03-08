using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;
using UnrealBinaryBuilder.Avalonia.Classes;

namespace UnrealBinaryBuilder.Avalonia.Views.UserControls;

public partial class DownloadDialog : Window
{
    private long _currentFileSize;
    public string? VersionText = null;
    public string? ChangelogUrl = "https://github.com/Olli1080/Unreal-Binary-Builder/blob/master/CHANGELOG.md";

    public DownloadDialog(string InVersion)
    {
        InitializeComponent();
        DownloadProgressbar.IsVisible = false;
        DownloadNowBtn.IsVisible = CancelBtn.IsVisible = true;
        VersionText = InVersion;
        DownloadProgressTextBlock.Text = $"Download {VersionText}? You are running {UnrealBinaryBuilderHelpers.GetProductVersionString()}";
    }

    public DownloadDialog()
    {
        InitializeComponent();
    }

    public void Initialize(long fileSize)
    {
        _currentFileSize = fileSize;
        DownloadProgressbar.IsIndeterminate = false;
        DownloadProgressbar.Maximum = 100;
        DownloadProgressbar.Value = 0;
        DownloadNowBtn.IsEnabled = CancelBtn.IsEnabled = false;
        DownloadProgressTextBlock.Text = "Downloading...";
        DownloadProgressbar.IsVisible = true;
    }

    public void SetProgress(int InProgress)
    {
        DownloadProgressbar.Value = InProgress;
        DownloadProgressTextBlock.Text = $"Downloading {VersionText} - {DownloadProgressbar.Value}/{DownloadProgressbar.Maximum} (File Size: {PostBuildSettings.BytesToString(_currentFileSize)})";
    }

    private void DownloadNowBtn_Click(object? sender, RoutedEventArgs e)
    {
        // Close the dialog and signal the main window to start the download
        Close(true);
    }

    private void CancelBtn_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void ViewChangelog_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(ChangelogUrl)) return;

        var url = $"{ChangelogUrl}#{VersionText?.Replace(".", "")}";
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // If it fails, try a different approach (e.g., Windows-specific)
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                Process.Start("explorer", url);
            }
        }
    }
}
