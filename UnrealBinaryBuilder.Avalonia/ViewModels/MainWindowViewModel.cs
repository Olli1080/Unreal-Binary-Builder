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
    private readonly IVelopackUpdaterService _updater;
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
    private readonly ITelemetryService _telemetryService;
    private readonly IBuildPipeline _buildPipeline;
    private readonly ILocalizationService _localizationService;
    
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
    [ObservableProperty] private ObservableCollection<BuildPreset> _presets = new();
    [ObservableProperty] private DashboardViewModel _dashboard;

    [ObservableProperty] private ObservableCollection<LanguageInfo> _availableLanguages = new();
    [ObservableProperty] private LanguageInfo? _selectedLanguage;

    partial void OnSelectedLanguageChanged(LanguageInfo? value) {
        if (value != null && !string.IsNullOrEmpty(value.Code) && (Settings == null || value.Code != Settings.Language)) {
            if (Settings != null) Settings.Language = value.Code;
            _localizationService.SetLanguage(value.Code);
            if (Settings != null) _settingsService.SaveSettings(Settings);
        }
    }

    // Version dependencies
    [ObservableProperty] private bool _supportWin32;
    [ObservableProperty] private bool _supportConsoles;
    [ObservableProperty] private bool _supportHTML5;
    [ObservableProperty] private bool _supportServerClientTargets;
    [ObservableProperty] private bool _isEngineSelection425OrAbove;
    [ObservableProperty] private bool _supportLinuxAArch64;
    [ObservableProperty] private bool _supportLinuxArm64;
    [ObservableProperty] private bool _supportWinArm64;
    [ObservableProperty] private bool _supportVisionOS;

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
        App.Current?.Services?.GetRequiredService<IVelopackUpdaterService>() ?? throw new InvalidOperationException("VelopackUpdater not found"),
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
        App.Current?.Services?.GetRequiredService<ILogFormatterService>() ?? throw new InvalidOperationException("LogFormatter not found"),
        App.Current?.Services?.GetRequiredService<ITelemetryService>() ?? throw new InvalidOperationException("TelemetryService not found"),
        App.Current?.Services?.GetRequiredService<IBuildPipeline>() ?? throw new InvalidOperationException("BuildPipeline not found"),
        App.Current?.Services?.GetRequiredService<ILocalizationService>() ?? throw new InvalidOperationException("LocalizationService not found")
    ) { }

    public MainWindowViewModel(
        IProcessExecutor processExecutor, IVelopackUpdaterService updater, IPlatformService platformService, ISettingsService settingsService, 
        IUnrealEngineProvider ueProvider, IUBBLogger logger, UiLogSink uiLogSink, IUIService uiService, 
        ISetupService setupService, IZipService zipService, IEngineBuildService engineBuildService, 
        IPluginBuildService pluginBuildService, IGitService gitService, IBuildTimerService timerService, 
        IBuildHistoryService historyService, IBuildOrchestrationService orchestrationService, ILogFormatterService logFormatter,
        ITelemetryService telemetryService, IBuildPipeline buildPipeline, ILocalizationService localizationService)
    {
        _processExecutor = processExecutor; _updater = updater; _platformService = platformService;
        _settingsService = settingsService; _ueProvider = ueProvider; _logger = logger;
        _uiService = uiService; _setupService = setupService; _zipService = zipService;
        _engineBuildService = engineBuildService; _pluginBuildService = pluginBuildService;
        _gitService = gitService; _timerService = timerService; _historyService = historyService; 
        _orchestrationService = orchestrationService; _logFormatter = logFormatter;
        _telemetryService = telemetryService;
        _buildPipeline = buildPipeline;
        _localizationService = localizationService;

        Settings = _settingsService.GetSettings();
        Presets = new ObservableCollection<BuildPreset>(Settings.Presets);

        foreach (var lang in _localizationService.GetAvailableLanguages()) {
            AvailableLanguages.Add(lang);
        }
        SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.Code == Settings.Language);

        foreach (BuildConfiguration config in Enum.GetValues(typeof(BuildConfiguration)))
            GameConfigWrappers.Add(new GameConfigWrapper(Settings.GameConfigurations, config, () => _settingsService.SaveSettings(Settings)));

        string[] p = { "Win64", "Win32", "Mac", "Linux", "LinuxAArch64", "Android", "IOS", "HTML5", "TVOS", "Switch", "PS4", "XboxOne", "Lumin", "HoleLens" };
        foreach (var name in p) PluginPlatforms.Add(new PluginPlatformWrapper(name, name == "Win64"));

        _timerService.ElapsedChanged += (s, e) => ElapsedTime = e;
        uiLogSink.OnLog += OnLogReceived;

        Dashboard = new DashboardViewModel(_historyService);

        LoadVisualStudio();
        UpdateVersionDependencies();
        LoadHistory();
    }

    [RelayCommand] private async Task SavePreset() {
        var name = await _uiService.ShowMessageDialog("Save Preset", "Enter a name for this build configuration:", "Save", null, "Cancel");
        // Note: Realistically we need a text input dialog here. Since our IUIService doesn't have one yet, 
        // I'll assume for this prototype we'd implement or use a simple convention.
        // For now, let's use a dummy name if we can't get one easily, but the goal is clear.
        
        // Let's improve the IUIService later. For now, let's prompt for a simple name.
        string presetName = $"Preset {Presets.Count + 1}"; 
        
        var snapshot = JsonConvert.DeserializeObject<BuilderSettingsJson>(JsonConvert.SerializeObject(Settings));
        if (snapshot != null) {
            var preset = new BuildPreset(presetName, snapshot);
            Settings.Presets.Add(preset);
            Presets.Add(preset);
            _settingsService.SaveSettings(Settings);
            _uiService.ShowToast($"Preset '{presetName}' saved.");
        }
    }

    [RelayCommand] private void LoadPreset(BuildPreset preset) {
        if (preset == null) return;
        var snapshot = JsonConvert.DeserializeObject<BuilderSettingsJson>(JsonConvert.SerializeObject(preset.Settings));
        if (snapshot != null) {
            // We keep global app settings like theme and window size from current
            snapshot.Theme = Settings.Theme;
            snapshot.Language = Settings.Language;
            snapshot.WindowWidth = Settings.WindowWidth;
            snapshot.WindowHeight = Settings.WindowHeight;
            snapshot.WindowLeft = Settings.WindowLeft;
            snapshot.WindowTop = Settings.WindowTop;
            snapshot.WindowMaximized = Settings.WindowMaximized;
            snapshot.Presets = Settings.Presets; // Keep the list

            Settings = snapshot;
            OnPropertyChanged(nameof(Settings));
            UpdateVersionDependencies();
            _uiService.ShowToast($"Preset '{preset.Name}' loaded.");
        }
    }

    [RelayCommand] private void DeletePreset(BuildPreset preset) {
        if (preset == null) return;
        Settings.Presets.Remove(preset);
        Presets.Remove(preset);
        _settingsService.SaveSettings(Settings);
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
            SupportWinArm64 = _engineMetadata.SupportWinArm64; SupportVisionOS = _engineMetadata.SupportVisionOS;
        }
        GitInfo = _gitService.GetGitInfo(EnginePath);
    }

    [ObservableProperty] private bool _isUpdateAvailable = false;

    [RelayCommand] private async Task CheckForUpdates() { 
        _telemetryService.TrackEvent("Update:Check"); 
        await _updater.CheckForUpdatesAsync();
        IsUpdateAvailable = _updater.IsUpdateAvailable;
        if (IsUpdateAvailable) {
            if (await _uiService.ShowMessageDialog("Update Available", "A new version is available. Would you like to download it now?", "Yes", null, "No") == UBBDialogResult.Primary) {
                await DownloadUpdate();
            }
        }
    }

    [RelayCommand] private async Task DownloadUpdate() {
        StatusText = "Downloading update...";
        await _updater.DownloadUpdatesAsync();
        StatusText = "Update ready to install.";
        if (await _uiService.ShowMessageDialog("Update Ready", "The update has been downloaded. Restart the application to apply it?", "Restart Now", null, "Later") == UBBDialogResult.Primary) {
            _updater.ApplyUpdatesAndRestart();
        }
    }
    public string SelectedCategoryTag => (SelectedCategory as FluentAvalonia.UI.Controls.NavigationViewItem)?.Tag?.ToString() ?? string.Empty;
    public string EnginePath { get => Settings.SetupBatFile ?? string.Empty; set { Settings.SetupBatFile = PathHelpers.NormalizePath(value); OnPropertyChanged(); UpdateVersionDependencies(); } }

    partial void OnSelectedCategoryChanged(object? value) {
        OnPropertyChanged(nameof(SelectedCategoryTag)); _telemetryService.TrackEvent($"Navigation:{SelectedCategoryTag}");
        if (SelectedCategoryTag == "Dashboard") _ = Dashboard.RefreshStatsAsync();
        switch (SelectedCategoryTag) { case "SourceCode": _ = GetSourceCode(); break; case "Support": _ = OpenSupport(); break; case "Changelog": _ = OpenChangelog(); break; case "About": OpenAbout(); break; }
    }

    [RelayCommand] private async Task BrowseEnginePath() { var path = await _uiService.BrowseFolderAsync("Select Unreal Engine Root Folder"); if (path != null) { EnginePath = path; _settingsService.SaveSettings(Settings); } }
    [RelayCommand] private async Task BrowseCustomBuildFile() { var path = await _uiService.BrowseFileAsync("Select Custom Build XML File", new[] { "*.xml" }, "XML Files"); if (path != null) { Settings.CustomBuildFile = path; _settingsService.SaveSettings(Settings); OnPropertyChanged(nameof(Settings)); _telemetryService.TrackEvent($"BuildXML:Custom:{Path.GetFileName(path)}"); } }
    [RelayCommand] private void ResetDefaultBuildXML() { Settings.CustomBuildFile = null; _settingsService.SaveSettings(Settings); OnPropertyChanged(nameof(Settings)); _telemetryService.TrackEvent("BuildXML:ResetToDefault"); }
    [RelayCommand] private async Task BrowsePluginPath() { var path = await _uiService.BrowseFileAsync("Select .uplugin file", new[] { "*.uplugin" }, "Unreal Plugin"); if (path != null) PluginPath = path; }
    [RelayCommand] private async Task BrowsePluginDestinationPath() { var path = await _uiService.BrowseFolderAsync("Select Output Folder"); if (path != null) PluginDestinationPath = path; }
    [RelayCommand] private async Task BrowsePluginZipPath() { var path = await _uiService.BrowseFolderAsync("Select Zip Output Folder"); if (path != null) PluginZipPath = path; }
    [RelayCommand] private async Task BrowseZipPath() { string? hash = _gitService.GetCommitHashShort(EnginePath); var path = await _uiService.SaveFileAsync("Select Zip Save Location", hash ?? "EngineBuild", ".zip", "Zip File"); if (path != null) { Settings.ZipEnginePath = PathHelpers.NormalizePath(path); _settingsService.SaveSettings(Settings); OnPropertyChanged(nameof(Settings)); } }

    [RelayCommand] private void AddPlugin() {
        if (string.IsNullOrEmpty(PluginPath) || string.IsNullOrEmpty(PluginDestinationPath)) return;
        List<string>? platforms = PluginOverridePlatforms ? PluginPlatforms.Where(p => p.IsChecked).Select(p => p.Name).ToList() : null;
        var vm = new PluginCardViewModel(PluginPath, PluginDestinationPath, EnginePath, "Current", _platformService) { TargetPlatforms = platforms, CanZip = PluginZip, TargetZipPath = PluginZipPath, ZipForMarketplaceZip = PluginZipForMarketplace };
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
                    await BuildEngine();
                    return;
                }
            }
        }

        if (!File.Exists(Path.Combine(EnginePath, "Setup.bat")) && !File.Exists(Path.Combine(EnginePath, "Setup.sh"))) { await _uiService.ShowMessageDialog("Incorrect folder", "This is not the Unreal Engine root folder."); return; }
        
        await ExecuteEnginePipelineAsync();
    }

    [RelayCommand] private async Task BuildEngine() {
        if (IsBuilding || string.IsNullOrEmpty(EnginePath)) return;

        string? gitHash = _gitService.GetCommitHashShort(EnginePath);
        if (_orchestrationService.IsResumable(EnginePath, gitHash, Settings)) {
            var state = _orchestrationService.LoadState();
            if (state != null && state.CompletedStages.Contains(BuildStage.Build)) {
                 if (await _uiService.ShowMessageDialog("Resume Build", "A previous build was partially completed (Compilation is done). Resume from Packaging (Zip)?", "Yes", null, "No") == UBBDialogResult.Primary) {
                    await ExecuteEnginePipelineAsync();
                    return;
                }
            }
        }

        if (Settings.WithHTML5 && Settings.ShowHTML5DeprecatedMessage && !SupportHTML5) { await _uiService.ShowMessageDialog("Deprecated", "HTML5 support was removed."); Settings.WithHTML5 = false; }
        if (Settings.WithSwitch && Settings.ShowConsoleDeprecatedMessage && !SupportConsoles) { await _uiService.ShowMessageDialog("Deprecated", "Console support was removed."); Settings.WithSwitch = false; }
        if (Settings.WithWin64NoPCH && await _uiService.ShowMessageDialog("Warning", "Building without PCH will take a long time. Continue?", "Yes", null, "No") != UBBDialogResult.Primary) return;
        if (Settings.EnableEngineBuildConfirmationMessage && await _uiService.ShowMessageDialog("Build Binary Version", "This is a long process. Continue?", "Yes", null, "No") != UBBDialogResult.Primary) return;
        
        _orchestrationService.ClearState();
        await ExecuteEnginePipelineAsync();
    }

    private async Task ExecuteEnginePipelineAsync() {
        IsBuilding = true; _timerService.Restart(); LogText = string.Empty; _logFormatter.Reset();
        
        var progress = new Progress<BuildPipelineProgress>(p => {
            StatusText = p.Message;
            CurrentStage = p.Stage switch {
                BuildPipelineStage.Setup or BuildPipelineStage.ProjectFiles or BuildPipelineStage.AutomationTool => BuildStage.Setup,
                BuildPipelineStage.EngineBuild => BuildStage.Build,
                BuildPipelineStage.Zipping => BuildStage.Zip,
                BuildPipelineStage.Finished => BuildStage.Finished,
                BuildPipelineStage.Failed => BuildStage.Failed,
                _ => BuildStage.Idle
            };
        });

        bool success = await _buildPipeline.ExecuteEnginePipelineAsync(EnginePath, Settings, SelectedMsBuild, SelectedArchitecture, SelectedVsVersion, _engineMetadata, progress);
        
        IsBuilding = false; _timerService.Stop();
        if (success) _uiService.ShowToast(_localizationService.GetString("EngineBuildPipelineFinished"), UBBNotificationType.Success); 
        else _uiService.ShowToast(_localizationService.GetString("EngineBuildPipelineFailed"), UBBNotificationType.Error);
        
        await RecordHistoryAsync(success, StatusText, "Engine");
        if (success) _orchestrationService.ClearState();
    }

    [RelayCommand] private async Task BuildPlugins() {
        if (IsBuilding) return; if (PluginQueue.Count == 0) { await _uiService.ShowMessageDialog("Queue Empty", "Queue is empty."); return; }
        IsBuilding = true; CurrentStage = BuildStage.Build; _timerService.Restart(); LogText = string.Empty; _logFormatter.Reset();
        _uiService.ShowToast($"Building {PluginQueue.Count} plugins.", UBBNotificationType.Info);
        
        var progress = new Progress<BuildPipelineProgress>(p => {
            StatusText = p.Message;
            if (p.Stage == BuildPipelineStage.Finished) CurrentStage = BuildStage.Finished;
            else if (p.Stage == BuildPipelineStage.Failed) CurrentStage = BuildStage.Failed;
        });

        bool success = await _buildPipeline.ExecutePluginPipelineAsync(PluginQueue, progress);
        
        IsBuilding = false; _timerService.Stop();
        if (success) _uiService.ShowToast("Plugin builds finished.", UBBNotificationType.Success); 
        else _uiService.ShowToast("Plugin builds failed.", UBBNotificationType.Error);
        
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
        await Dashboard.RefreshStatsAsync();
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
    [RelayCommand] private void OpenLogFolder() => _settingsService.OpenLogFolder();
    [RelayCommand] private void RemovePlugin(PluginCardViewModel p) => PluginQueue.Remove(p);

    private async Task OpenUrlAsync(string url) {
        if (await _uiService.ShowMessageDialog("External Link", $"This will open your browser to:\n\n{url}\n\nDo you want to continue?", "Open", null, "Cancel") == UBBDialogResult.Primary) {
            _uiService.OpenUrl(url);
        }
    }

    [RelayCommand] private async Task GetSourceCode() => await OpenUrlAsync("https://github.com/EpicGames/UnrealEngine");
    [RelayCommand] private async Task OpenSupport() => await OpenUrlAsync("https://github.com/Olli1080/Unreal-Binary-Builder");
    [RelayCommand] private async Task OpenChangelog() => await OpenUrlAsync("https://github.com/Olli1080/Unreal-Binary-Builder/blob/master/CHANGELOG.md");
    
    [RelayCommand] private void OpenAbout() => _uiService.ShowAboutDialog();
    [RelayCommand] private void OpenSettings() => _settingsService.OpenSettings();
}
