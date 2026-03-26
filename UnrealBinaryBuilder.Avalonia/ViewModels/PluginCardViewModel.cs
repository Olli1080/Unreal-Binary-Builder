using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Media.Imaging;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

namespace UnrealBinaryBuilder.Avalonia.ViewModels;

public partial class PluginCardViewModel : ViewModelBase
{
    private readonly IPlatformService _platformService;

    [ObservableProperty] private string _pluginName = string.Empty;
    [ObservableProperty] private string _pluginDescription = "No description available.";
    [ObservableProperty] private string _engineVersion = string.Empty;
    [ObservableProperty] private Bitmap? _pluginIcon;
    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private bool _isSuccess = false;
    [ObservableProperty] private bool _isFailed = false;
    [ObservableProperty] private bool _showZipProgress = false;

    public string PluginPath { get; init; } = string.Empty;
    public string DestinationPath { get; init; } = string.Empty;
    public string RunUATFile { get; init; } = string.Empty;

    // Build configuration
    public List<string>? TargetPlatforms { get; set; }
    public bool CanZip { get; set; }
    public string TargetZipPath { get; set; } = string.Empty;
    public bool ZipForMarketplaceZip { get; set; }

    public event EventHandler? RemoveRequested;

    public PluginCardViewModel(string inPluginPath, string inDestination, string inEnginePath, string inEngineName, IPlatformService platformService)
    {
        _platformService = platformService;
        PluginPath = PathHelpers.NormalizePath(inPluginPath);
        DestinationPath = PathHelpers.NormalizePath(inDestination);
        RunUATFile = PathHelpers.ToUnixPath(Path.Combine(inEnginePath, "Engine", "Build", "BatchFiles", "RunUAT.bat"));
        EngineVersion = inEngineName;
        PluginName = Path.GetFileNameWithoutExtension(PluginPath);

        LoadPluginData();
    }

    private void LoadPluginData()
    {
        try {
            if (File.Exists(PluginPath)) {
                using (StreamReader reader = File.OpenText(PluginPath)) {
                    JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                    PluginDescription = o.GetValue("Description")?.ToString() ?? "No description available.";
                }
                string pluginDir = PathHelpers.GetParentDirectory(PluginPath);
                string iconPath = PathHelpers.ToUnixPath(Path.Combine(pluginDir, "Resources", "Icon128.png"));
                if (File.Exists(iconPath)) PluginIcon = new Bitmap(iconPath);
            }
        } catch (Exception ex) { Debug.WriteLine($"Error loading plugin data: {ex.Message}"); }
    }

    [RelayCommand]
    private void OpenDestination()
    {
        _platformService.OpenFolder(DestinationPath);
    }

    [RelayCommand] private void Cancel() => RemoveRequested?.Invoke(this, EventArgs.Empty);

    public void StartBuild() { IsLoading = true; IsSuccess = false; IsFailed = false; }
    public void FinishBuild(bool success) { IsLoading = false; IsSuccess = success; IsFailed = !success; }
}
