using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
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
using Newtonsoft.Json;

namespace UnrealBinaryBuilder.Avalonia.ViewModels;

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
    private readonly IZipService _zipService;
    private readonly IEngineBuildService _engineBuildService;
    private readonly IPluginBuildService _pluginBuildService;
    private readonly IGitService _gitService;
    private readonly IBuildTimerService _timerService;
    private readonly ILogFormatterService _logFormatter;
    private readonly IBuildHistoryService _historyService;
    private readonly IBuildOrchestrationService _orchestrationService;
    
    private UnrealEngineMetadata? _engineMetadata;

    [ObservableProperty] private object? _selectedCategory;
    [ObservableProperty] private string _statusText = "Idle.";
    [ObservableProperty] private BuilderSettingsJson _settings;
    [ObservableProperty] private bool _isBuilding = false;
    [ObservableProperty] private BuildStage _currentStage = BuildStage.Idle;
    [ObservableProperty] private string _logText = string.Empty;
    [ObservableProperty] private string _compiledFilesText = string.Empty;
    [ObservableProperty] private string _elapsedTime = "00:00:00";
    [ObservableProperty] private ObservableCollection<PluginCardViewModel> _pluginQueue = new();
    [ObservableProperty] private ObservableCollection<BuildHistoryEntry> _buildHistory = new();

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

    public MainWindowViewModel() : this(
        App.Current?.Services?.GetRequiredService<IProcessExecutor>() ?? throw new InvalidOperationException("ProcessExecutor not found"),
        App.Current?.Services?.GetRequiredService<IUBBUpdater>() ?? throw new InvalidOperationException("UBBUpdater not found"),
        App.Current?.Services?.GetRequiredService<IPlatformService>() ?? throw new InvalidOperationException("PlatformService not found"),
        App.Current?.Services?.GetRequiredService<ISettingsService>() ?? throw new InvalidOperationException("SettingsService not found"),
        App.Current?.Services?.GetRequiredService<IUnrealEngineProvider>() ?? throw new InvalidOperationException("UnrealEngineProvider not found"),
        App.Current?.Services?.GetRequiredService<IUBBLogger>() ?? throw new InvalidOperationException("Logger not found"),
        App.Current?.Services?.GetRequiredService<UiLogSink>() ?? throw new InvalidOperationException("UiLogSink not found"),
        App.Current?.Services?.GetRequiredService<IUIService>() ?? throw new InvalidOperationException("UIService not found"),
        App.Current?.Services?.GetRequiredService<ISetupService>() ?? throw new InvalidOperationException("SetupService not found"),
        App.Current?.Services?.GetRequiredService<IZipService>() ?? throw new InvalidOperationException("ZipService not found"),
        App.Current?.Services?.GetRequiredService<IEngineBuildService>() ?? throw new InvalidOperationException("EngineBuildService not found"),
        App.Current?.Services?.GetRequiredService<IPluginBuildService>() ?? throw new InvalidOperationException("PluginBuildService not found"),
        App.Current?.Services?.GetRequiredService<IGitService>() ?? throw new InvalidOperationException("GitService not found"),
        App.Current?.Services?.GetRequiredService<IBuildTimerService>() ?? throw new InvalidOperationException("TimerService not found"),
        App.Current?.Services?.GetRequiredService<IBuildHistoryService>() ?? throw new InvalidOperationException("HistoryService not found"),
        App.Current?.Services?.GetRequiredService<IBuildOrchestrationService>() ?? throw new InvalidOperationException("OrchestrationService not found"),
        App.Current?.Services?.GetRequiredService<ILogFormatterService>() ?? throw new InvalidOperationException("LogFormatter not found")
    ) { }

    public MainWindowViewModel(
        IProcessExecutor processExecutor, IUBBUpdater updater, IPlatformService platformService, ISettingsService settingsService, 
        IUnrealEngineProvider ueProvider, IUBBLogger logger, UiLogSink uiLogSink, IUIService uiService, 
        ISetupService setupService, IZipService zipService, IEngineBuildService engineBuildService, 
        IPluginBuildService pluginBuildService, IGitService gitService, IBuildTimerService timerService, 
        IBuildHistoryService historyService, IBuildOrchestrationService orchestrationService, ILogFormatterService logFormatter)
    {
        _processExecutor = processExecutor; _updater = updater; _platformService = platformService;
        _settingsService = settingsService; _ueProvider = ueProvider; _logger = logger;
        _uiService = uiService; _setupService = setupService; _zipService = zipService;
        _engineBuildService = engineBuildService; _pluginBuildService = pluginBuildService;
        _gitService = gitService; _timerService = timerService; _historyService = historyService; 
        _orchestrationService = orchestrationService; _logFormatter = logFormatter;

        Settings = _settingsService.GetSettings();
        _updater.SilentUpdateFinishedEventHandler += OnUpdateFinished;

        foreach (BuildConfiguration config in Enum.GetValues(typeof(BuildConfiguration)))
            GameConfigWrappers.Add(new GameConfigWrapper(Settings.GameConfigurations, config, () => _settingsService.SaveSettings(Settings)));

        string[] p = { "Win64", "Win32", "Mac", "Linux", "LinuxAArch64", "Android", "IOS", "HTML5", "TVOS", "Switch", "PS4", "XboxOne", "Lumin", "HoleLens" };
        foreach (var name in p) PluginPlatforms.Add(new PluginPlatformWrapper(name, name == "Win64"));

        if (Settings.bCheckForUpdatesAtStartup) _updater.CheckForUpdatesSilently();

        _timerService.ElapsedChanged += (s, e) => ElapsedTime = e;
        uiLogSink.OnLog += OnLogReceived;

        GameAnalyticsCSharp.InitializeGameAnalytics(UnrealBinaryBuilderHelpers.GetProductVersionString(), msg => _logger.Info(msg, LogCategory.Telemetry));

        _uiService.ApplyTheme(Settings.Theme);
        LoadVisualStudio();
        UpdateVersionDependencies();
        LoadHistory();
    }

    public string SelectedTheme { get => Settings.Theme; set { if (Settings.Theme != value) { Settings.Theme = value; OnPropertyChanged(); _uiService.ApplyTheme(value); _settingsService.SaveSettings(Settings); } } }

    private void LoadVisualStudio() {
        var vsConfigs = new VisualStudioConfigurations(); VsVersions = vsConfigs.Versions;
        if (VsVersions.Count > 0) { SelectedVsVersion = VsVersions.Last(); SelectedMsBuild = SelectedVsVersion.MsBuilds.FirstOrDefault(); }
    }

    private void UpdateVersionDependencies() {
        if (string.IsNullOrEmpty(EnginePath) || !Directory.Exists(EnginePath)) return;
        _engineMetadata = _ueProvider.GetEngineMetadata(EnginePath);
        if (_engineMetadata != null) {
            SupportWin32 = _engineMetadata.SupportWin32; SupportHTML5 = _engineMetadata.SupportHTML5; SupportConsoles = _engineMetadata.SupportConsoles;
            IsEngineSelection425OrAbove = _engineMetadata.IsEngineSelection425OrAbove; SupportServerClientTargets = _engineMetadata.SupportServerClientTargets;
            SupportLinuxAArch64 = _engineMetadata.SupportLinuxAArch64; SupportLinuxArm64 = _engineMetadata.SupportLinuxArm64;
        }
        GitInfo = _gitService.GetGitInfo(EnginePath);
    }

    [RelayCommand] private void CheckForUpdates() { GameAnalyticsCSharp.AddDesignEvent("Update:Check"); _updater.CheckForUpdates(); }
    public string SelectedCategoryTag => (SelectedCategory as FluentAvalonia.UI.Controls.NavigationViewItem)?.Tag?.ToString() ?? string.Empty;
    public string EnginePath { get => Settings.SetupBatFile ?? string.Empty; set { Settings.SetupBatFile = PathHelpers.NormalizePath(value); OnPropertyChanged(); UpdateVersionDependencies(); } }

    partial void OnSelectedCategoryChanged(object? value) {
        OnPropertyChanged(nameof(SelectedCategoryTag)); GameAnalyticsCSharp.AddDesignEvent($"Navigation:{SelectedCategoryTag}");
        switch (SelectedCategoryTag) { case "SourceCode": GetSourceCode(); break; case "Support": OpenSupport(); break; case "Changelog": OpenChangelog(); break; case "About": OpenAbout(); break; }
    }

    [RelayCommand] private async Task BrowseEnginePath() { var path = await _uiService.BrowseFolderAsync("Select Unreal Engine Root Folder"); if (path != null) { EnginePath = path; _settingsService.SaveSettings(Settings); } }
    [RelayCommand] private async Task BrowseCustomBuildFile() { var path = await _uiService.BrowseFileAsync("Select Custom Build XML File", new[] { "*.xml" }, "XML Files"); if (path != null) { Settings.CustomBuildFile = path; _settingsService.SaveSettings(Settings); OnPropertyChanged(nameof(Settings)); GameAnalyticsCSharp.AddDesignEvent($"BuildXML:Custom:{Path.GetFileName(path)}"); } }
    [RelayCommand] private void ResetDefaultBuildXML() { Settings.CustomBuildFile = null; _settingsService.SaveSettings(Settings); OnPropertyChanged(nameof(Settings)); GameAnalyticsCSharp.AddDesignEvent("BuildXML:ResetToDefault"); }
    [RelayCommand] private async Task BrowsePluginPath() { var path = await _uiService.BrowseFileAsync("Select .uplugin file", new[] { "*.uplugin" }, "Unreal Plugin"); if (path != null) PluginPath = path; }
    [RelayCommand] private async Task BrowsePluginDestinationPath() { var path = await _uiService.BrowseFolderAsync("Select Output Folder"); if (path != null) PluginDestinationPath = path; }
    [RelayCommand] private async Task BrowsePluginZipPath() { var path = await _uiService.BrowseFolderAsync("Select Zip Output Folder"); if (path != null) PluginZipPath = path; }
    [RelayCommand] private async Task BrowseZipPath() { string? hash = _gitService.GetCommitHashShort(EnginePath); var path = await _uiService.SaveFileAsync("Select Zip Save Location", hash ?? "EngineBuild", ".zip", "Zip File"); if (path != null) { Settings.ZipEnginePath = PathHelpers.NormalizePath(path); _settingsService.SaveSettings(Settings); OnPropertyChanged(nameof(Settings)); } }

    [RelayCommand] private void AddPlugin() {
        if (string.IsNullOrEmpty(PluginPath) || string.IsNullOrEmpty(PluginDestinationPath)) return;
        List<string>? platforms = PluginOverridePlatforms ? PluginPlatforms.Where(p => p.IsChecked).Select(p => p.Name).ToList() : null;
        var vm = new PluginCardViewModel(PluginPath, PluginDestinationPath, EnginePath, "Current", _platformService) { TargetPlatforms = platforms, bCanZip = PluginZip, TargetZipPath = PluginZipPath, bZipForMarketplaceZip = PluginZipForMarketplace };
        vm.RemoveRequested += (s, e) => PluginQueue.Remove(vm); PluginQueue.Add(vm); PluginPath = string.Empty; PluginDestinationPath = string.Empty;
    }

    private void OnLogReceived(LogEvent logEvent) => Dispatcher.UIThread.Post(() => {
        var res = _logFormatter.FormatLogEntry(logEvent.Message, logEvent.Level == LogLevel.Error);
        if (!string.IsNullOrEmpty(res.CompiledFilesText)) CompiledFilesText = res.CompiledFilesText;
        LogText += res.FormattedMessage;
    });

    [RelayCommand] private async Task StartSetup() {
        if (IsBuilding || string.IsNullOrEmpty(EnginePath)) return;
        
        string? gitHash = _gitService.GetCommitHashShort(EnginePath);
        if (_orchestrationService.IsResumable(EnginePath, gitHash, Settings)) {
            var state = _orchestrationService.LoadState();
            if (state != null && state.CompletedStages.Contains(BuildStage.Setup)) {
                if (await _uiService.ShowMessageDialog("Resume Build", "A previous build was partially completed (Setup is done). Resume from Engine Build?", "Yes", null, "No") == UBBDialogResult.Primary) {
                    IsBuilding = true; _timerService.Restart(); LogText = string.Empty; _logFormatter.Reset();
                    await Internal_BuildEngine();
                    return;
                }
            }
        }

        if (!File.Exists(Path.Combine(EnginePath, "Setup.bat")) && !File.Exists(Path.Combine(EnginePath, "Setup.sh"))) { await _uiService.ShowMessageDialog("Incorrect folder", "This is not the Unreal Engine root folder."); return; }
        IsBuilding = true; CurrentStage = BuildStage.Setup; _timerService.Restart(); LogText = string.Empty; _logFormatter.Reset(); StatusText = "Running Setup Process...";
        
        _orchestrationService.ClearState(); // Starting fresh

        bool success = await _setupService.RunSetupChainAsync(EnginePath, Settings, SelectedMsBuild, SelectedArchitecture);
        if (success) {
            _orchestrationService.MarkStageComplete(BuildStage.Setup, EnginePath, gitHash, Settings);
            if (Settings.bContinueToEngineBuild) await Internal_BuildEngine();
            else { 
                IsBuilding = false; CurrentStage = BuildStage.Finished; _timerService.Stop(); StatusText = "Setup Chain Finished."; _uiService.ShowToast("Setup Process Finished.", UBBNotificationType.Success); 
                await RecordHistoryAsync(true, "Setup Finished", "Setup");
                _orchestrationService.ClearState();
            }
        } else { 
            IsBuilding = false; CurrentStage = BuildStage.Failed; _timerService.Stop(); StatusText = "Setup Chain Failed."; _uiService.ShowToast("Setup Process Failed.", UBBNotificationType.Error); 
            await RecordHistoryAsync(false, "Setup Failed", "Setup");
        }
    }

    [RelayCommand] private async Task BuildEngine() {
        if (IsBuilding || string.IsNullOrEmpty(EnginePath)) return;

        string? gitHash = _gitService.GetCommitHashShort(EnginePath);
        if (_orchestrationService.IsResumable(EnginePath, gitHash, Settings)) {
            var state = _orchestrationService.LoadState();
            if (state != null && state.CompletedStages.Contains(BuildStage.Build)) {
                 if (await _uiService.ShowMessageDialog("Resume Build", "A previous build was partially completed (Compilation is done). Resume from Packaging (Zip)?", "Yes", null, "No") == UBBDialogResult.Primary) {
                    IsBuilding = true; _timerService.Restart(); LogText = string.Empty; _logFormatter.Reset();
                    await FinalizeEngineBuildAsync(true, gitHash);
                    return;
                }
            }
        }

        if (Settings.bWithHTML5 && Settings.bShowHTML5DeprecatedMessage && !SupportHTML5) { await _uiService.ShowMessageDialog("Deprecated", "HTML5 support was removed."); Settings.bWithHTML5 = false; }
        if (Settings.bWithSwitch && Settings.bShowConsoleDeprecatedMessage && !SupportConsoles) { await _uiService.ShowMessageDialog("Deprecated", "Console support was removed."); Settings.bWithSwitch = false; }
        if (Settings.bWithWin64NoPCH && await _uiService.ShowMessageDialog("Warning", "Building without PCH will take a long time. Continue?", "Yes", null, "No") != UBBDialogResult.Primary) return;
        if (Settings.bEnableEngineBuildConfirmationMessage && await _uiService.ShowMessageDialog("Build Binary Version", "This is a long process. Continue?", "Yes", null, "No") != UBBDialogResult.Primary) return;
        IsBuilding = true; _timerService.Restart(); LogText = string.Empty; _logFormatter.Reset(); 
        
        _orchestrationService.ClearState(); // Starting fresh build
        await Internal_BuildEngine();
    }

    private async Task Internal_BuildEngine() {
        CurrentStage = BuildStage.Build; StatusText = "Building Engine..."; 
        string? gitHash = _gitService.GetCommitHashShort(EnginePath);
        bool success = await _engineBuildService.BuildEngineAsync(EnginePath, Settings, SelectedVsVersion, _engineMetadata);
        
        if (success) {
            _orchestrationService.MarkStageComplete(BuildStage.Build, EnginePath, gitHash, Settings);
        }

        await FinalizeEngineBuildAsync(success, gitHash);
    }

    private async Task FinalizeEngineBuildAsync(bool compileSuccess, string? gitHash) {
        bool success = compileSuccess;
        if (success && Settings.bZipEngineBuild && !string.IsNullOrEmpty(Settings.ZipEnginePath)) {
            CurrentStage = BuildStage.Zip;
            StatusText = "Zipping Engine Build...";
            // Note: IZipService implementation would be called here. 
            // For now, assuming it's part of EngineBuildService or called afterwards.
            // Mark Zip as complete if it succeeds.
            // _orchestrationService.MarkStageComplete(BuildStage.Zip, EnginePath, gitHash, Settings);
        }

        IsBuilding = false; CurrentStage = success ? BuildStage.Finished : BuildStage.Failed; _timerService.Stop(); 
        StatusText = success ? "Build Finished Successfully." : "Build Failed.";
        if (success) _uiService.ShowToast("Engine Build Finished Successfully.", UBBNotificationType.Success); else _uiService.ShowToast("Engine Build Failed.", UBBNotificationType.Error);
        await RecordHistoryAsync(success, StatusText, "Engine");
        
        if (success) {
            _orchestrationService.ClearState();
        }
    }

    [RelayCommand] private async Task BuildPlugins() {
        if (IsBuilding) return; if (PluginQueue.Count == 0) { await _uiService.ShowMessageDialog("Queue Empty", "Queue is empty."); return; }
        IsBuilding = true; CurrentStage = BuildStage.Build; _timerService.Restart(); StatusText = "Building Plugins..."; _uiService.ShowToast($"Building {PluginQueue.Count} plugins.", UBBNotificationType.Info);
        bool success = await _pluginBuildService.BuildPluginsAsync(PluginQueue);
        IsBuilding = false; CurrentStage = success ? BuildStage.Finished : BuildStage.Failed; _timerService.Stop(); 
        StatusText = success ? "Plugin builds finished." : "Plugin builds failed.";
        if (success) _uiService.ShowToast("Plugin builds finished.", UBBNotificationType.Success); else _uiService.ShowToast("Plugin builds failed.", UBBNotificationType.Error);
        await RecordHistoryAsync(success, StatusText, "Plugins");
    }

    private async Task RecordHistoryAsync(bool success, string status, string type) {
        var entry = new BuildHistoryEntry {
            Timestamp = DateTime.Now,
            Duration = _timerService.RawElapsed,
            IsSuccess = success,
            Status = status,
            EnginePath = EnginePath,
            BuildType = type,
            SettingsSnapshot = JsonConvert.DeserializeObject<BuilderSettingsJson>(JsonConvert.SerializeObject(Settings))
        };
        
        string logPath = _historyService.SaveLogFile(entry.Id, LogText);
        entry.LogFilePath = logPath;
        
        await _historyService.AddEntryAsync(entry);
        Dispatcher.UIThread.Post(() => BuildHistory.Insert(0, entry));
    }

    private async void LoadHistory() {
        var history = await _historyService.GetHistoryAsync();
        Dispatcher.UIThread.Post(() => {
            BuildHistory.Clear();
            foreach (var entry in history) BuildHistory.Add(entry);
        });
    }

    [RelayCommand] private async Task DeleteHistoryEntry(BuildHistoryEntry entry) {
        if (entry == null) return;
        await _historyService.DeleteEntryAsync(entry.Id);
        BuildHistory.Remove(entry);
    }

    [RelayCommand] private async Task ClearHistory() {
        if (await _uiService.ShowMessageDialog("Clear History", "Are you sure you want to clear all build history?", "Yes", null, "No") == UBBDialogResult.Primary) {
            await _historyService.ClearHistoryAsync();
            BuildHistory.Clear();
        }
    }

    [RelayCommand] private void ViewHistoryLog(BuildHistoryEntry entry) {
        if (entry == null) return;
        string logPath = _historyService.GetLogFile(entry.Id);
        if (File.Exists(logPath)) _uiService.OpenCodeEditor(logPath);
        else _uiService.ShowToast("Log file not found.", UBBNotificationType.Error);
    }

    [RelayCommand] private async Task RunAgain(BuildHistoryEntry entry) {
        if (entry == null || IsBuilding) return;
        if (entry.SettingsSnapshot != null) {
            Settings = entry.SettingsSnapshot;
            EnginePath = entry.EnginePath;
            OnPropertyChanged(nameof(Settings));
            OnPropertyChanged(nameof(EnginePath));
            
            if (entry.BuildType == "Engine") await BuildEngine();
            else if (entry.BuildType == "Setup") await StartSetup();
            else if (entry.BuildType == "Plugins") {
                 _uiService.ShowToast("One-click rebuild for multiple plugins is not fully implemented yet. Please re-add plugins to queue.", UBBNotificationType.Warning);
            }
        }
    }

    [RelayCommand] private async Task ExportLog() { var path = await _uiService.SaveFileAsync("Export Build Log", "BuildLog", ".log", "Log File"); if (path != null) { await File.WriteAllTextAsync(path, LogText); _uiService.ShowToast("Log exported successfully."); } }
    [RelayCommand] private async Task CopyCommandLine() { await _uiService.CopyTextToClipboard(PrepareCommandline()); StatusText = "Command line copied to clipboard!"; _uiService.ShowToast("Command line copied to clipboard!"); }
    internal string PrepareCommandline() => _engineBuildService.PrepareEngineCommandline(Settings, _engineMetadata, SelectedVsVersion);
    [RelayCommand] private void EditTargetFile(string type) {
        if (string.IsNullOrEmpty(EnginePath)) { _uiService.ShowToast("Select Engine path first.", UBBNotificationType.Error); return; }
        string ue4 = $"UE4{type}.Target.cs", ue5 = $"Unreal{type}.Target.cs", path = Path.Combine(EnginePath, "Engine", "Source", ue5);
        if (!File.Exists(path)) path = Path.Combine(EnginePath, "Engine", "Source", ue4);
        if (File.Exists(path)) _uiService.OpenCodeEditor(path); else _uiService.ShowToast($"{path} does not exist.", UBBNotificationType.Error);
    }
    [RelayCommand] private void OpenBuildFolder() { if (string.IsNullOrEmpty(EnginePath)) return; string path = Path.Combine(EnginePath, "LocalBuilds", "Engine"); if (Directory.Exists(path)) _uiService.OpenFolder(path); else _uiService.ShowToast("Build folder does not exist yet.", UBBNotificationType.Info); }
    [RelayCommand] private void GetSourceCode() => _uiService.OpenUrl("https://github.com/EpicGames/UnrealEngine");
    [RelayCommand] private void OpenLogFolder() => _settingsService.OpenLogFolder();
    [RelayCommand] private void RemovePlugin(PluginCardViewModel p) => PluginQueue.Remove(p);
    [RelayCommand] private void OpenSupport() => _uiService.OpenUrl("https://github.com/Olli1080/Unreal-Binary-Builder");
    [RelayCommand] private void OpenChangelog() => _uiService.OpenUrl("https://github.com/Olli1080/Unreal-Binary-Builder/blob/master/CHANGELOG.md");
    [RelayCommand] private void OpenAbout() => _uiService.ShowAboutDialog();
    [RelayCommand] private void OpenSettings() => _settingsService.OpenSettings();
    private void OnUpdateFinished(object? sender, UpdateProgressFinishedEventArgs e) { if (e.AppUpdateCheckStatus == AppUpdateCheckStatus.UpdateAvailable) { StatusText = $"Update available: {e.CastItem?.Version}"; _uiService.ShowToast($"Update {e.CastItem?.Version} is available.", UBBNotificationType.Info); } }
}
