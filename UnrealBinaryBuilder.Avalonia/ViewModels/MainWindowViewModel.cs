using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Classes.Logging;
using UnrealBinaryBuilder.Avalonia.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace UnrealBinaryBuilder.Avalonia.ViewModels;

public class GameConfigWrapper : ObservableObject
{
    private readonly HashSet<BuildConfiguration> _configurations;
    private readonly BuildConfiguration _config;
    private readonly Action _onChanged;

    public GameConfigWrapper(HashSet<BuildConfiguration> configurations, BuildConfiguration config, Action onChanged)
    {
        _configurations = configurations;
        _config = config;
        _onChanged = onChanged;
    }

    public bool IsChecked
    {
        get => _configurations.Contains(_config);
        set
        {
            if (value) _configurations.Add(_config);
            else _configurations.Remove(_config);
            OnPropertyChanged();
            _onChanged();
        }
    }

    public string Name => _config.ToString();
}

public class PluginPlatformWrapper : ObservableObject
{
    private bool _isChecked;
    public string Name { get; init; }
    public bool IsChecked { get => _isChecked; set => SetProperty(ref _isChecked, value); }
    public PluginPlatformWrapper(string name, bool isChecked = false) { Name = name; IsChecked = isChecked; }
}

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IProcessExecutor _processExecutor;
    private readonly IUBBUpdater _updater;
    private readonly IPlatformService _platformService;
    private readonly ISettingsService _settingsService;
    private readonly IUnrealEngineProvider _ueProvider;
    private readonly IUBBLogger _logger;
    private readonly IUIService _uiService;
    private readonly ISetupService _setupService;
    private readonly PostBuildSettings _postBuildSettings = new();
    private UnrealEngineMetadata? _engineMetadata;

    [ObservableProperty] private object? _selectedCategory;
    [ObservableProperty] private string _statusText = "Idle.";
    [ObservableProperty] private BuilderSettingsJson _settings;
    [ObservableProperty] private bool _isBuilding = false;
    [ObservableProperty] private string _logText = string.Empty;
    [ObservableProperty] private string _compiledFilesText = string.Empty;
    [ObservableProperty] private string _elapsedTime = "00:00:00";
    [ObservableProperty] private ObservableCollection<PluginCardViewModel> _pluginQueue = new();

    private readonly Stopwatch _buildStopwatch = new();
    private readonly DispatcherTimer _buildTimer = new(DispatcherPriority.Background);

    // Version dependencies
    [ObservableProperty] private bool _supportWin32;
    [ObservableProperty] private bool _supportConsoles;
    [ObservableProperty] private bool _supportHTML5;
    [ObservableProperty] private bool _supportServerClientTargets;
    [ObservableProperty] private bool _isEngineSelection425OrAbove;
    [ObservableProperty] private bool _supportLinuxAArch64;
    [ObservableProperty] private bool _supportLinuxArm64;

    // Visual Studio
    [ObservableProperty] private List<VisualStudioVersion> _vsVersions = new();
    [ObservableProperty] private VisualStudioVersion? _selectedVsVersion;
    [ObservableProperty] private VisualStudioMsBuild? _selectedMsBuild;
    [ObservableProperty] private string _selectedArchitecture = "x64";

    // Git Info
    [ObservableProperty] private string _gitInfo = "Not a git repository.";

    // Plugin Builder Additional Options
    [ObservableProperty] private bool _pluginOverridePlatforms;
    [ObservableProperty] private bool _pluginZip;
    [ObservableProperty] private bool _pluginZipForMarketplace = true;
    [ObservableProperty] private string _pluginZipPath = string.Empty;
    partial void OnPluginZipPathChanged(string value) => PluginZipPath = PathHelpers.NormalizePath(value);

    [ObservableProperty] private string _pluginPath = string.Empty;
    partial void OnPluginPathChanged(string value) => PluginPath = PathHelpers.NormalizePath(value);

    [ObservableProperty] private string _pluginDestinationPath = string.Empty;
    partial void OnPluginDestinationPathChanged(string value) => PluginDestinationPath = PathHelpers.NormalizePath(value);
    public ObservableCollection<PluginPlatformWrapper> PluginPlatforms { get; } = new();

    public List<GameConfigWrapper> GameConfigWrappers { get; } = new();

    private int _compiledFiles = 0;
    private int _compiledFilesTotal = 0;

    public MainWindowViewModel() : this(
        App.Current?.Services?.GetRequiredService<IProcessExecutor>() ?? throw new InvalidOperationException("ProcessExecutor not found"),
        App.Current?.Services?.GetRequiredService<IUBBUpdater>() ?? throw new InvalidOperationException("UBBUpdater not found"),
        App.Current?.Services?.GetRequiredService<IPlatformService>() ?? throw new InvalidOperationException("PlatformService not found"),
        App.Current?.Services?.GetRequiredService<ISettingsService>() ?? throw new InvalidOperationException("SettingsService not found"),
        App.Current?.Services?.GetRequiredService<IUnrealEngineProvider>() ?? throw new InvalidOperationException("UnrealEngineProvider not found"),
        App.Current?.Services?.GetRequiredService<IUBBLogger>() ?? throw new InvalidOperationException("Logger not found"),
        App.Current?.Services?.GetRequiredService<UiLogSink>() ?? throw new InvalidOperationException("UiLogSink not found"),
        App.Current?.Services?.GetRequiredService<IUIService>() ?? throw new InvalidOperationException("UIService not found"),
        App.Current?.Services?.GetRequiredService<ISetupService>() ?? throw new InvalidOperationException("SetupService not found")
    ) { }

    public MainWindowViewModel(IProcessExecutor processExecutor, IUBBUpdater updater, IPlatformService platformService, ISettingsService settingsService, IUnrealEngineProvider ueProvider, IUBBLogger logger, UiLogSink uiLogSink, IUIService uiService, ISetupService setupService)
    {
        _processExecutor = processExecutor;
        _updater = updater;
        _platformService = platformService;
        _settingsService = settingsService;
        _ueProvider = ueProvider;
        _logger = logger;
        _uiService = uiService;
        _setupService = setupService;

        Settings = _settingsService.GetSettings();
        _updater.SilentUpdateFinishedEventHandler += OnUpdateFinished;

        foreach (BuildConfiguration config in Enum.GetValues(typeof(BuildConfiguration)))
        {
            GameConfigWrappers.Add(new GameConfigWrapper(Settings.GameConfigurations, config, () => _settingsService.SaveSettings(Settings)));
        }

        // Initialize plugin platforms
        string[] p = { "Win64", "Win32", "Mac", "Linux", "LinuxAArch64", "Android", "IOS", "HTML5", "TVOS", "Switch", "PS4", "XboxOne", "Lumin", "HoloLens" };
        foreach (var name in p) PluginPlatforms.Add(new PluginPlatformWrapper(name, name == "Win64"));

        if (Settings.bCheckForUpdatesAtStartup)
        {
            _updater.CheckForUpdatesSilently();
        }

        _buildTimer.Interval = TimeSpan.FromSeconds(1);
        _buildTimer.Tick += (s, e) => ElapsedTime = _buildStopwatch.Elapsed.ToString(@"hh\:mm\:ss");

        uiLogSink.OnLog += OnLogReceived;

        GameAnalyticsCSharp.InitializeGameAnalytics(UnrealBinaryBuilderHelpers.GetProductVersionString(), msg => _logger.Info(msg, LogCategory.Telemetry));

        ApplyTheme();
        LoadVisualStudio();
        UpdateVersionDependencies();
    }

    public string SelectedTheme
    {
        get => Settings.Theme;
        set { if (Settings.Theme != value) { Settings.Theme = value; OnPropertyChanged(); ApplyTheme(); _settingsService.SaveSettings(Settings); } }
    }

    private void ApplyTheme() => _uiService.ApplyTheme(Settings.Theme);

    private void LoadVisualStudio()
    {
        var vsConfigs = new VisualStudioConfigurations();
        VsVersions = vsConfigs.Versions;
        if (VsVersions.Count > 0) {
            SelectedVsVersion = VsVersions.Last();
            SelectedMsBuild = SelectedVsVersion.MsBuilds.FirstOrDefault();
        }
    }

    private void UpdateGitInfo()
    {
        if (string.IsNullOrEmpty(EnginePath) || !Directory.Exists(EnginePath)) return;
        try {
            string? branch = Git.GetBranchName(EnginePath);
            string? hash = Git.GetCommitHashShort(EnginePath);
            if (branch != null && hash != null) GitInfo = $"Branch: {branch} | Hash: {hash}";
            else GitInfo = "Git not detected.";
        } catch { GitInfo = "Git error."; }
    }

    private void UpdateVersionDependencies()
    {
        if (string.IsNullOrEmpty(EnginePath) || !Directory.Exists(EnginePath)) return;
        _engineMetadata = _ueProvider.GetEngineMetadata(EnginePath);
        if (_engineMetadata != null) {
            SupportWin32 = _engineMetadata.SupportWin32;
            SupportHTML5 = _engineMetadata.SupportHTML5;
            SupportConsoles = _engineMetadata.SupportConsoles;
            IsEngineSelection425OrAbove = _engineMetadata.IsEngineSelection425OrAbove;
            SupportServerClientTargets = _engineMetadata.SupportServerClientTargets;
            SupportLinuxAArch64 = _engineMetadata.SupportLinuxAArch64;
            SupportLinuxArm64 = _engineMetadata.SupportLinuxArm64;
        }
        UpdateGitInfo();
    }

    private void ShowToast(string message, UBBNotificationType type = UBBNotificationType.Info, string title = "") => _uiService.ShowToast(message, type, title);
    
    private async Task<UBBDialogResult> ShowMessageDialog(string title, string content, string primaryButton = "OK", string? secondaryButton = null, string? closeButton = null)
    {
        return await _uiService.ShowMessageDialog(title, content, primaryButton, secondaryButton, closeButton);
    }

    private void StartTiming() { _buildStopwatch.Restart(); _buildTimer.Start(); }
    private void StopTiming() { _buildStopwatch.Stop(); _buildTimer.Stop(); }

    private void OnUpdateFinished(object? sender, UpdateProgressFinishedEventArgs e)
    {
        if (e.AppUpdateCheckStatus == AppUpdateCheckStatus.UpdateAvailable) {
            StatusText = $"Update available: {e.CastItem?.Version}";
            ShowToast($"Update {e.CastItem?.Version} is available.", UBBNotificationType.Info);
        }
    }

    [RelayCommand] private void CheckForUpdates() { GameAnalyticsCSharp.AddDesignEvent("Update:Check"); _updater.CheckForUpdates(); }

    public string SelectedCategoryTag => (SelectedCategory as FluentAvalonia.UI.Controls.NavigationViewItem)?.Tag?.ToString() ?? string.Empty;

    public string EnginePath {
        get => Settings.SetupBatFile ?? string.Empty;
        set { Settings.SetupBatFile = PathHelpers.NormalizePath(value); OnPropertyChanged(); UpdateVersionDependencies(); }
    }

    partial void OnSelectedCategoryChanged(object? value)
    {
        OnPropertyChanged(nameof(SelectedCategoryTag));
        GameAnalyticsCSharp.AddDesignEvent($"Navigation:{SelectedCategoryTag}");
        switch (SelectedCategoryTag) {
            case "SourceCode": GetSourceCode(); break;
            case "Support": OpenSupport(); break;
            case "Changelog": OpenChangelog(); break;
            case "About": OpenAbout(); break;
        }
    }

    [RelayCommand]
    private async Task BrowseEnginePath()
    {
        var path = await _uiService.BrowseFolderAsync("Select Unreal Engine Root Folder");
        if (path != null) { EnginePath = path; _settingsService.SaveSettings(Settings); }
    }

    [RelayCommand]
    private async Task BrowseCustomBuildFile()
    {
        var path = await _uiService.BrowseFileAsync("Select Custom Build XML File", new[] { "*.xml" }, "XML Files");
        if (path != null) { Settings.CustomBuildFile = path; _settingsService.SaveSettings(Settings); OnPropertyChanged(nameof(Settings)); GameAnalyticsCSharp.AddDesignEvent($"BuildXML:Custom:{Path.GetFileName(path)}"); }
    }

    [RelayCommand] private void ResetDefaultBuildXML() { Settings.CustomBuildFile = null; _settingsService.SaveSettings(Settings); OnPropertyChanged(nameof(Settings)); GameAnalyticsCSharp.AddDesignEvent("BuildXML:ResetToDefault"); }

    [RelayCommand]
    private async Task BrowsePluginPath()
    {
        var path = await _uiService.BrowseFileAsync("Select .uplugin file", new[] { "*.uplugin" }, "Unreal Plugin");
        if (path != null) PluginPath = path;
    }

    [RelayCommand]
    private async Task BrowsePluginDestinationPath()
    {
        var path = await _uiService.BrowseFolderAsync("Select Output Folder");
        if (path != null) PluginDestinationPath = path;
    }

    [RelayCommand]
    private async Task BrowsePluginZipPath()
    {
        var path = await _uiService.BrowseFolderAsync("Select Zip Output Folder");
        if (path != null) PluginZipPath = path;
    }

    [RelayCommand]
    private async Task BrowseZipPath()
    {
        string? hash = Git.GetCommitHashShort(EnginePath);
        string suggestedName = string.IsNullOrEmpty(hash) ? "EngineBuild" : hash;
        var path = await _uiService.SaveFileAsync("Select Zip Save Location", suggestedName, ".zip", "Zip File");
        if (path != null) { Settings.ZipEnginePath = PathHelpers.NormalizePath(path); _settingsService.SaveSettings(Settings); OnPropertyChanged(nameof(Settings)); }
    }

    [RelayCommand]
    private void AddPlugin()
    {
        if (string.IsNullOrEmpty(PluginPath) || string.IsNullOrEmpty(PluginDestinationPath)) return;
        List<string>? platforms = PluginOverridePlatforms ? PluginPlatforms.Where(p => p.IsChecked).Select(p => p.Name).ToList() : null;
        var vm = new PluginCardViewModel(PluginPath, PluginDestinationPath, EnginePath, "Current", _platformService) {
            TargetPlatforms = platforms, bCanZip = PluginZip, TargetZipPath = PluginZipPath, bZipForMarketplaceZip = PluginZipForMarketplace
        };
        vm.RemoveRequested += (s, e) => PluginQueue.Remove(vm);
        PluginQueue.Add(vm);
        PluginPath = string.Empty; PluginDestinationPath = string.Empty;
    }

    private void OnLogReceived(LogEvent logEvent)
    {
        Dispatcher.UIThread.Post(() => {
            AddLogEntryToUI(logEvent.Message, logEvent.Level == LogLevel.Error);
        });
    }

    private void AddLogEntryToUI(string message, bool isError = false)
    {
        if (string.IsNullOrEmpty(message)) return;
        const string sp = @"\*{6} \[(\d+)\/(\d+)\]", pp = @"\w.+\.(cpp|cc|c|h|ispc)";
        if (Regex.IsMatch(message, sp)) { var m = Regex.Match(message, sp); _compiledFiles = 0; if (int.TryParse(m.Groups[2].Value, out int t)) _compiledFilesTotal = t; }
        if (Regex.IsMatch(message, pp)) { _compiledFiles++; CompiledFilesText = $"[Compiled: {_compiledFiles}/{_compiledFilesTotal}]"; }
        LogText += (isError ? "[ERROR] " : "") + message + "\n";
    }

    [RelayCommand]
    private async Task StartSetup()
    {
        if (IsBuilding || string.IsNullOrEmpty(EnginePath)) return;

        if (!File.Exists(Path.Combine(EnginePath, "Setup.bat")))
        {
            await ShowMessageDialog("Incorrect folder", $"This is not the Unreal Engine root folder.\n\nPlease select the root folder where {UnrealBinaryBuilderHelpers.SetupBatFileName} and {UnrealBinaryBuilderHelpers.GenerateProjectBatFileName} exists.");
            return;
        }

        IsBuilding = true; StartTiming(); LogText = string.Empty;
        StatusText = "Running Setup Process...";

        bool chainSuccess = await _setupService.RunSetupChainAsync(EnginePath, Settings, SelectedMsBuild, SelectedArchitecture);

        if (chainSuccess && Settings.bContinueToEngineBuild) await Internal_BuildEngine();
        else { 
            IsBuilding = false; StopTiming(); StatusText = chainSuccess ? "Setup Chain Finished." : "Setup Chain Failed."; 
            if (chainSuccess) ShowToast("Setup Process Finished.", UBBNotificationType.Success); 
            else { ShowToast("Setup Process Failed.", UBBNotificationType.Error); }
        }
    }

    [RelayCommand]
    private async Task BuildEngine() 
    { 
        if (IsBuilding || string.IsNullOrEmpty(EnginePath)) return; 

        if (Settings.bWithHTML5 && Settings.bShowHTML5DeprecatedMessage && !SupportHTML5)
        {
            await ShowMessageDialog("Deprecated", "HTML5 support was removed from Unreal Engine 4.24 and higher. You had it enabled but since it is of no use, it is disabled.");
            Settings.bWithHTML5 = false;
        }

        if (Settings.bWithSwitch && Settings.bShowConsoleDeprecatedMessage && !SupportConsoles)
        {
            await ShowMessageDialog("Deprecated", "Console support was removed from Unreal Engine 4.25 and higher. You had it enabled but since it is of no use, it is disabled.");
            Settings.bWithSwitch = false; Settings.bWithPS4 = false; Settings.bWithXboxOne = false;
        }

        if (Settings.bWithWin64NoPCH)
        {
            var res = await ShowMessageDialog("Warning", "Building with Precompiled Headers disabled will take a long time. Are you sure you want to continue?", "Yes", null, "No");
            if (res != UBBDialogResult.Primary) return;
        }

        if (Settings.bEnableEngineBuildConfirmationMessage)
        {
            var res = await ShowMessageDialog("Build Binary Version", "You are going to build a binary version of Unreal Engine. This is a long process and might take time to finish. Are you sure you want to continue?", "Yes", null, "No");
            if (res != UBBDialogResult.Primary) return;
        }

        if (Settings.bWithDDC && Settings.bEnableDDCMessages)
        {
            var res = await ShowMessageDialog("Warning", "Building Derived Data Cache (DDC) is one of the slowest aspect of the build. You can skip this step if you want to. Do you want to continue with DDC enabled?", "Yes", "No", "Cancel");
            if (res == UBBDialogResult.None || res == UBBDialogResult.Secondary) Settings.bWithDDC = false;
            else if (res == UBBDialogResult.Primary) { /* Keep DDC */ }
            else return; // Cancel
        }

        IsBuilding = true; StartTiming(); LogText = string.Empty; await Internal_BuildEngine(); 
    }

    private async Task Internal_BuildEngine()
    {
        StatusText = "Building Engine..."; GameAnalyticsCSharp.AddDesignEvent("Build:Started"); GameAnalyticsCSharp.AddProgressStart("Build", "Engine");

        var metadata = _ueProvider.GetEngineMetadata(EnginePath);
        string automationPath = _ueProvider.GetAutomationPath(EnginePath, metadata?.IsUE5 ?? false);

        int ec = await _processExecutor.ExecuteAsync(automationPath, PrepareCommandline(), EnginePath, LogCategory.Build);
        bool success = ec == 0; GameAnalyticsCSharp.AddProgressEnd("Build", "Engine", !success);
        if (success && Settings.bZipEngineBuild && !string.IsNullOrEmpty(Settings.ZipEnginePath)) { 
            StatusText = "Zipping build..."; 
            GameAnalyticsCSharp.AddDesignEvent("Zip:Started");
            await _postBuildSettings.SaveToZip(Path.Combine(EnginePath, "LocalBuilds", "Engine"), Settings.ZipEnginePath, Settings); 
            GameAnalyticsCSharp.AddDesignEvent("Zip:Finished");
        }
        IsBuilding = false; StopTiming(); StatusText = success ? "Build Finished Successfully." : "Build Failed."; if (success) ShowToast("Engine Build Finished Successfully.", UBBNotificationType.Success); else { ShowToast("Engine Build Failed.", UBBNotificationType.Error); _logger.Error("Engine Build Failed.", LogCategory.Build); }
        if (success && Settings.bShutdownIfBuildSuccess && Settings.bShutdownPC) Internal_ShutdownPC();
    }

    private void Internal_ShutdownPC() 
    { 
        _logger.Info("Shutting down PC in 5 seconds...", LogCategory.General); 
        GameAnalyticsCSharp.AddDesignEvent("Shutdown:Started"); 
        _platformService.ShutdownPC(5);
        Environment.Exit(0); 
    }

    [RelayCommand]
    private async Task BuildPlugins()
    {
        if (IsBuilding) return;
        if (PluginQueue.Count == 0) { await ShowMessageDialog("Queue Empty", "Queue is empty. Add one or more plugin to queue and build."); return; }

        IsBuilding = true; StartTiming(); ShowToast($"Building {PluginQueue.Count} plugins.", UBBNotificationType.Info);
        foreach (var plugin in PluginQueue.ToList()) {
            StatusText = $"Building {plugin.PluginName}..."; GameAnalyticsCSharp.AddProgressStart("Build", "Plugin");
            plugin.StartBuild();
            string args = BuildArgumentBuilder.BuildPluginArguments(plugin).ToString();
            int ec = await _processExecutor.ExecuteAsync(plugin.RunUATFile, args, Path.GetDirectoryName(plugin.RunUATFile)!, LogCategory.Build);
            bool success = ec == 0;
            if (success && plugin.bCanZip) {
                GameAnalyticsCSharp.AddDesignEvent($"ZipPlugin:Started:{plugin.PluginName}");
                await _postBuildSettings.SavePluginToZip(plugin.PluginPath, plugin.TargetZipPath, plugin.bZipForMarketplaceZip, true);
                GameAnalyticsCSharp.AddDesignEvent($"ZipPlugin:Finished:{plugin.PluginName}");
            }
            plugin.FinishBuild(success); GameAnalyticsCSharp.AddProgressEnd("Build", "Plugin", !success);
            if (!success) { _logger.Error($"Plugin Build Failed: {plugin.PluginName}", LogCategory.Build); break; }
        }
        IsBuilding = false; StopTiming(); StatusText = "Plugin builds finished."; ShowToast("Plugin builds finished.", UBBNotificationType.Success);
    }

    [RelayCommand]
    private async Task ExportLog()
    {
        var path = await _uiService.SaveFileAsync("Export Build Log", "BuildLog", ".log", "Log File");
        if (path != null) { await File.WriteAllTextAsync(path, LogText); ShowToast("Log exported successfully."); _logger.Info($"Log exported to: {path}", LogCategory.General); }
    }

    [RelayCommand]
    private async Task CopyCommandLine() { await _uiService.CopyTextToClipboard(PrepareCommandline()); StatusText = "Command line copied to clipboard!"; ShowToast("Command line copied to clipboard!"); GameAnalyticsCSharp.AddDesignEvent("CommandLine:CopyToClipboard"); }

    internal string PrepareCommandline()
    {
        return BuildArgumentBuilder.BuildEngineArguments(Settings, _engineMetadata, SelectedVsVersion).ToString();
    }

    [Obsolete("Use IUBBLogger instead")]
    public void AddLogEntry(string message, bool isError = false)
    {
        if (isError) _logger.Error(message, LogCategory.Build);
        else _logger.Info(message, LogCategory.Build);
    }

    [RelayCommand]
    private void EditTargetFile(string type)
    {
        if (string.IsNullOrEmpty(EnginePath)) { ShowToast("Select Engine path first.", UBBNotificationType.Error); return; }
        string ue4 = $"UE4{type}.Target.cs", ue5 = $"Unreal{type}.Target.cs";
        string path = Path.Combine(EnginePath, "Engine", "Source", ue5);
        if (!File.Exists(path)) path = Path.Combine(EnginePath, "Engine", "Source", ue4);
        if (File.Exists(path)) _uiService.OpenCodeEditor(path);
        else ShowToast($"{path} does not exist.", UBBNotificationType.Error);
    }

    [RelayCommand]
    private void OpenBuildFolder()
    {
        if (string.IsNullOrEmpty(EnginePath)) return;
        string path = Path.Combine(EnginePath, "LocalBuilds", "Engine");
        if (Directory.Exists(path)) _uiService.OpenFolder(path);
        else ShowToast("Build folder does not exist yet.", UBBNotificationType.Info);
    }

    [RelayCommand] private void GetSourceCode() => _uiService.OpenUrl("https://github.com/EpicGames/UnrealEngine");
    [RelayCommand] private void OpenLogFolder() => _settingsService.OpenLogFolder();
    [RelayCommand] private void RemovePlugin(PluginCardViewModel p) => PluginQueue.Remove(p);
    [RelayCommand] private void OpenSupport() => _uiService.OpenUrl("https://github.com/ryanjon2040/Unreal-Binary-Builder");
    [RelayCommand] private void OpenChangelog() => _uiService.OpenUrl("https://github.com/ryanjon2040/Unreal-Binary-Builder/blob/master/CHANGELOG.md");
    [RelayCommand] private void OpenAbout() => _uiService.ShowAboutDialog();
    [RelayCommand] private void OpenSettings() => _settingsService.OpenSettings();
}
