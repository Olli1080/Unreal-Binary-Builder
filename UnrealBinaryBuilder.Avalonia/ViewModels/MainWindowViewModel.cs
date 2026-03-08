using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System;
using System.Linq;
using Avalonia.Styling;
using UnrealBinaryBuilder.Avalonia.Views;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
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

public enum UBBNotificationType { Info, Success, Warning, Error }
public record NotificationEventArgs(string Title, string Message, UBBNotificationType Type);

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IProcessExecutor _processExecutor;
    private readonly IUBBUpdater _updater;
    private readonly IPlatformService _platformService;
    private readonly ISettingsService _settingsService;
    private readonly IUnrealEngineProvider _ueProvider;
    private readonly PostBuildSettings _postBuildSettings = new();

    [ObservableProperty] private object? _selectedCategory;
    [ObservableProperty] private string _statusText = "Idle.";
    [ObservableProperty] private BuilderSettingsJson _settings;
    [ObservableProperty] private bool _isBuilding = false;
    [ObservableProperty] private string _logText = string.Empty;
    [ObservableProperty] private string _compiledFilesText = string.Empty;
    [ObservableProperty] private string _elapsedTime = "00:00:00";
    [ObservableProperty] private ObservableCollection<PluginCardViewModel> _pluginQueue = new();

    public event EventHandler<NotificationEventArgs>? ShowNotification;

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
    private string? _logMessageErrors = null;

    public MainWindowViewModel() : this(
        App.Current?.Services?.GetRequiredService<IProcessExecutor>() ?? new ProcessExecutor(),
        App.Current?.Services?.GetRequiredService<IUBBUpdater>() ?? new UBBUpdater(),
        App.Current?.Services?.GetRequiredService<IPlatformService>() ?? (OperatingSystem.IsWindows() ? new WindowsPlatformService() : new LinuxPlatformService()),
        App.Current?.Services?.GetRequiredService<ISettingsService>() ?? new SettingsService(new WindowsPlatformService()),
        App.Current?.Services?.GetRequiredService<IUnrealEngineProvider>() ?? new UnrealEngineProvider()
    ) { }

    public MainWindowViewModel(IProcessExecutor processExecutor, IUBBUpdater updater, IPlatformService platformService, ISettingsService settingsService, IUnrealEngineProvider ueProvider)
    {
        _processExecutor = processExecutor;
        _updater = updater;
        _platformService = platformService;
        _settingsService = settingsService;
        _ueProvider = ueProvider;

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

        GameAnalyticsCSharp.InitializeGameAnalytics(UnrealBinaryBuilderHelpers.GetProductVersionString(), msg => AddLogEntry(msg));

        ApplyTheme();
        LoadVisualStudio();
        UpdateVersionDependencies();
    }

    public string SelectedTheme
    {
        get => Settings.Theme;
        set { if (Settings.Theme != value) { Settings.Theme = value; OnPropertyChanged(); ApplyTheme(); _settingsService.SaveSettings(Settings); } }
    }

    private void ApplyTheme()
    {
        if (Application.Current == null) return;
        GameAnalyticsCSharp.AddDesignEvent($"Theme:{Settings.Theme}");
        switch (Settings.Theme?.ToLower()) {
            case "light": Application.Current.RequestedThemeVariant = ThemeVariant.Light; break;
            case "dark": Application.Current.RequestedThemeVariant = ThemeVariant.Dark; break;
            default: Application.Current.RequestedThemeVariant = ThemeVariant.Default; break;
        }
    }

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
        var metadata = _ueProvider.GetEngineMetadata(EnginePath);
        if (metadata != null) {
            SupportWin32 = metadata.SupportWin32;
            SupportHTML5 = metadata.SupportHTML5;
            SupportConsoles = metadata.SupportConsoles;
            IsEngineSelection425OrAbove = metadata.IsEngineSelection425OrAbove;
            SupportServerClientTargets = metadata.SupportServerClientTargets;
            SupportLinuxAArch64 = metadata.SupportLinuxAArch64;
            SupportLinuxArm64 = metadata.SupportLinuxArm64;
        }
        UpdateGitInfo();
    }

    private void ShowToast(string message, UBBNotificationType type = UBBNotificationType.Info, string title = "") => ShowNotification?.Invoke(this, new NotificationEventArgs(title, message, type));
    
    private async Task<ContentDialogResult> ShowMessageDialog(string title, string content, string primaryButton = "OK", string? secondaryButton = null, string? closeButton = null)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
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
            return await dialog.ShowAsync();
        }
        return ContentDialogResult.None;
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

    private IStorageProvider? GetStorageProvider() => (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) ? desktop.MainWindow?.StorageProvider : null;

    [RelayCommand]
    private async Task BrowseEnginePath()
    {
        var sp = GetStorageProvider();
        if (sp == null) return;
        var res = await sp.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Select Unreal Engine Root Folder", AllowMultiple = false });
        if (res.Count > 0) { EnginePath = res[0].Path.LocalPath; _settingsService.SaveSettings(Settings); }
    }

    [RelayCommand]
    private async Task BrowseCustomBuildFile()
    {
        var sp = GetStorageProvider();
        if (sp == null) return;
        var res = await sp.OpenFilePickerAsync(new FilePickerOpenOptions { Title = "Select Custom Build XML File", FileTypeFilter = new[] { new FilePickerFileType("XML Files") { Patterns = new[] { "*.xml" } } }, AllowMultiple = false });
        if (res.Count > 0) { Settings.CustomBuildFile = res[0].Path.LocalPath; _settingsService.SaveSettings(Settings); OnPropertyChanged(nameof(Settings)); GameAnalyticsCSharp.AddDesignEvent($"BuildXML:Custom:{Path.GetFileName(Settings.CustomBuildFile)}"); }
    }

    [RelayCommand] private void ResetDefaultBuildXML() { Settings.CustomBuildFile = null; _settingsService.SaveSettings(Settings); OnPropertyChanged(nameof(Settings)); GameAnalyticsCSharp.AddDesignEvent("BuildXML:ResetToDefault"); }

    [RelayCommand]
    private async Task BrowsePluginPath()
    {
        var sp = GetStorageProvider();
        if (sp == null) return;
        var res = await sp.OpenFilePickerAsync(new FilePickerOpenOptions { Title = "Select .uplugin file", FileTypeFilter = new[] { new FilePickerFileType("Unreal Plugin") { Patterns = new[] { "*.uplugin" } } } });
        if (res.Count > 0) PluginPath = res[0].Path.LocalPath;
    }

    [RelayCommand]
    private async Task BrowsePluginDestinationPath()
    {
        var sp = GetStorageProvider();
        if (sp == null) return;
        var res = await sp.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Select Output Folder" });
        if (res.Count > 0) PluginDestinationPath = res[0].Path.LocalPath;
    }

    [RelayCommand]
    private async Task BrowsePluginZipPath()
    {
        var sp = GetStorageProvider();
        if (sp == null) return;
        var res = await sp.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Select Zip Output Folder" });
        if (res.Count > 0) PluginZipPath = res[0].Path.LocalPath;
    }

    [RelayCommand]
    private async Task BrowseZipPath()
    {
        var sp = GetStorageProvider();
        if (sp == null) return;
        var res = await sp.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Select Zip Save Location",
            SuggestedFileName = string.IsNullOrEmpty(Git.GetCommitHashShort(EnginePath)) ? "EngineBuild" : Git.GetCommitHashShort(EnginePath),
            FileTypeChoices = new[] { new FilePickerFileType("Zip File") { Patterns = new[] { "*.zip" } } },
            DefaultExtension = ".zip"
        });
        if (res != null) { Settings.ZipEnginePath = PathHelpers.NormalizePath(res.Path.LocalPath); _settingsService.SaveSettings(Settings); OnPropertyChanged(nameof(Settings)); }
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

    [RelayCommand]
    private async Task StartSetup()
    {
        if (IsBuilding || string.IsNullOrEmpty(EnginePath)) return;
        
        if (!File.Exists(Path.Combine(EnginePath, "Setup.bat")))
        {
            await ShowMessageDialog("Incorrect folder", $"This is not the Unreal Engine root folder.\n\nPlease select the root folder where {UnrealBinaryBuilderHelpers.SetupBatFileName} and {UnrealBinaryBuilderHelpers.GenerateProjectBatFileName} exists.");
            return;
        }

        IsBuilding = true; StartTiming(); LogText = string.Empty; _logMessageErrors = null;
        AddLogEntry($"Starting Build Chain in: {EnginePath}");
        bool chainSuccess = true;
        if (Settings.bBuildSetupBatFile) {
            StatusText = "Running Setup.bat..."; GameAnalyticsCSharp.AddProgressStart("Build", "Setup");
            int ec = await _processExecutor.ExecuteAsync(Path.Combine(EnginePath, "Setup.bat"), SetupBatCommandLineArgs(), EnginePath, msg => AddLogEntry(msg), msg => AddLogEntry(msg, true));
            chainSuccess = ec == 0; GameAnalyticsCSharp.AddProgressEnd("Build", "Setup", !chainSuccess);
        }
        if (chainSuccess && Settings.bGenerateProjectFiles) {
            StatusText = "Generating Project Files..."; GameAnalyticsCSharp.AddProgressStart("Build", "ProjectFiles");
            int ec = await _processExecutor.ExecuteAsync(Path.Combine(EnginePath, "GenerateProjectFiles.bat"), string.Empty, EnginePath, msg => AddLogEntry(msg), msg => AddLogEntry(msg, true));
            chainSuccess = ec == 0; GameAnalyticsCSharp.AddProgressEnd("Build", "ProjectFiles", !chainSuccess);
        }
        if (chainSuccess && Settings.bBuildAutomationTool) {
            StatusText = "Building AutomationTool..."; GameAnalyticsCSharp.AddProgressStart("Build", "AutomationTool");
            if (SelectedMsBuild != null) {
                string msbuildPath = SelectedArchitecture == "x64" ? SelectedMsBuild.X64Path : SelectedMsBuild.X32Path;
                string slnPath = Path.Combine(EnginePath, "Engine", "Source", "Programs", "AutomationTool", "AutomationTool.sln");
                if (File.Exists(slnPath)) {
                    int ec = await _processExecutor.ExecuteAsync(msbuildPath, $"\"{slnPath}\" /p:Configuration=Development /p:Platform=AnyCPU", EnginePath, msg => AddLogEntry(msg), msg => AddLogEntry(msg, true));
                    chainSuccess = ec == 0;
                }
            }
            GameAnalyticsCSharp.AddProgressEnd("Build", "AutomationTool", !chainSuccess);
        }
        if (chainSuccess && Settings.bContinueToEngineBuild) await Internal_BuildEngine();
        else { 
            IsBuilding = false; StopTiming(); StatusText = chainSuccess ? "Setup Chain Finished." : "Setup Chain Failed."; 
            if (chainSuccess) ShowToast("Setup Process Finished.", UBBNotificationType.Success); 
            else ShowToast("Setup Process Failed.", UBBNotificationType.Error);
            if (!string.IsNullOrEmpty(_logMessageErrors)) _settingsService.WriteErrorsToLogFile(_logMessageErrors);
        }
    }

    private string SetupBatCommandLineArgs()
    {
        string args = "--force";
        if (Settings.GitDependencyAll) args += " --all";
        foreach (var gp in Settings.GitDependencyPlatforms) if (!gp.bIsIncluded) args += $" --exclude={gp.Name}";
        args += $" --threads={Settings.GitDependencyThreads} --max-retries={Settings.GitDependencyMaxRetries}";
        if (!Settings.GitDependencyEnableCache) args += " --no-cache";
        else if (!string.IsNullOrEmpty(Settings.GitDependencyCache))
        {
            args += $" --cache={PathHelpers.ToUnixPath(Settings.GitDependencyCache)} --cache-size-multiplier={Settings.GitDependencyCacheMultiplier} --cache-days={Settings.GitDependencyCacheDays}";
        }
        if (!string.IsNullOrEmpty(Settings.GitDependencyProxy)) args += $" --proxy={Settings.GitDependencyProxy}";
        return args;
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
            if (res != ContentDialogResult.Primary) return;
        }

        if (Settings.bEnableEngineBuildConfirmationMessage)
        {
            var res = await ShowMessageDialog("Build Binary Version", "You are going to build a binary version of Unreal Engine. This is a long process and might take time to finish. Are you sure you want to continue?", "Yes", null, "No");
            if (res != ContentDialogResult.Primary) return;
        }

        if (Settings.bWithDDC && Settings.bEnableDDCMessages)
        {
            var res = await ShowMessageDialog("Warning", "Building Derived Data Cache (DDC) is one of the slowest aspect of the build. You can skip this step if you want to. Do you want to continue with DDC enabled?", "Yes", "No", "Cancel");
            if (res == ContentDialogResult.None || res == ContentDialogResult.Secondary) Settings.bWithDDC = false;
            else if (res == ContentDialogResult.Primary) { /* Keep DDC */ }
            else return; // Cancel
        }

        IsBuilding = true; StartTiming(); LogText = string.Empty; _logMessageErrors = null; await Internal_BuildEngine(); 
    }

    private async Task Internal_BuildEngine()
    {
        StatusText = "Building Engine..."; GameAnalyticsCSharp.AddDesignEvent("Build:Started"); GameAnalyticsCSharp.AddProgressStart("Build", "Engine");
        
        var metadata = _ueProvider.GetEngineMetadata(EnginePath);
        string automationPath = _ueProvider.GetAutomationPath(EnginePath, metadata?.IsUE5 ?? false);

        int ec = await _processExecutor.ExecuteAsync(automationPath, PrepareCommandline(), EnginePath, msg => AddLogEntry(msg), msg => AddLogEntry(msg, true));
        bool success = ec == 0; GameAnalyticsCSharp.AddProgressEnd("Build", "Engine", !success);
        if (success && Settings.bZipEngineBuild && !string.IsNullOrEmpty(Settings.ZipEnginePath)) { 
            StatusText = "Zipping build..."; 
            GameAnalyticsCSharp.AddDesignEvent("Zip:Started");
            await _postBuildSettings.SaveToZip(Path.Combine(EnginePath, "LocalBuilds", "Engine"), Settings.ZipEnginePath, Settings); 
            GameAnalyticsCSharp.AddDesignEvent("Zip:Finished");
        }
        IsBuilding = false; StopTiming(); StatusText = success ? "Build Finished Successfully." : "Build Failed."; if (success) ShowToast("Engine Build Finished Successfully.", UBBNotificationType.Success); else ShowToast("Engine Build Failed.", UBBNotificationType.Error);
        if (!string.IsNullOrEmpty(_logMessageErrors)) _settingsService.WriteErrorsToLogFile(_logMessageErrors);
        if (success && Settings.bShutdownIfBuildSuccess && Settings.bShutdownPC) Internal_ShutdownPC();
    }

    private void Internal_ShutdownPC() 
    { 
        AddLogEntry("Shutting down PC in 5 seconds..."); 
        GameAnalyticsCSharp.AddDesignEvent("Shutdown:Started"); 
        _platformService.ShutdownPC(5);
        Environment.Exit(0); 
    }

    [RelayCommand]
    private async Task BuildPlugins()
    {
        if (IsBuilding) return;
        if (PluginQueue.Count == 0) { await ShowMessageDialog("Queue Empty", "Queue is empty. Add one or more plugin to queue and build."); return; }

        IsBuilding = true; StartTiming(); _logMessageErrors = null; ShowToast($"Building {PluginQueue.Count} plugins.", UBBNotificationType.Info);
        foreach (var plugin in PluginQueue.ToList()) {
            StatusText = $"Building {plugin.PluginName}..."; GameAnalyticsCSharp.AddProgressStart("Build", "Plugin");
            plugin.StartBuild();
            string pArgs = string.Join("+", plugin.TargetPlatforms ?? new List<string> { "Win64" });
            string args = $"BuildPlugin -Plugin=\"{plugin.PluginPath}\" -Package=\"{plugin.DestinationPath}\" -Rocket -TargetPlatforms={pArgs}";
            int ec = await _processExecutor.ExecuteAsync(plugin.RunUATFile, args, Path.GetDirectoryName(plugin.RunUATFile)!, msg => AddLogEntry(msg), msg => AddLogEntry(msg, true));
            bool success = ec == 0;
            if (success && plugin.bCanZip) {
                GameAnalyticsCSharp.AddDesignEvent($"ZipPlugin:Started:{plugin.PluginName}");
                await _postBuildSettings.SavePluginToZip(plugin.PluginPath, plugin.TargetZipPath, plugin.bZipForMarketplaceZip, true);
                GameAnalyticsCSharp.AddDesignEvent($"ZipPlugin:Finished:{plugin.PluginName}");
            }
            plugin.FinishBuild(success); GameAnalyticsCSharp.AddProgressEnd("Build", "Plugin", !success);
            if (!success) break;
        }
        IsBuilding = false; StopTiming(); StatusText = "Plugin builds finished."; ShowToast("Plugin builds finished.", UBBNotificationType.Success);
        if (!string.IsNullOrEmpty(_logMessageErrors)) _settingsService.WriteErrorsToLogFile(_logMessageErrors);
    }

    [RelayCommand]
    private async Task ExportLog()
    {
        var sp = GetStorageProvider();
        if (sp == null) return;
        var res = await sp.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Build Log",
            SuggestedFileName = "BuildLog",
            DefaultExtension = ".log",
            FileTypeChoices = new[] { new FilePickerFileType("Log File") { Patterns = new[] { "*.log", "*.txt" } } }
        });
        if (res != null) { await File.WriteAllTextAsync(res.Path.LocalPath, LogText); ShowToast("Log exported successfully."); }
    }

    [RelayCommand]
    private async Task CopyCommandLine() { if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow?.Clipboard != null) { await desktop.MainWindow.Clipboard.SetTextAsync(PrepareCommandline()); StatusText = "Command line copied to clipboard!"; ShowToast("Command line copied to clipboard!"); GameAnalyticsCSharp.AddDesignEvent("CommandLine:CopyToClipboard"); } }

    internal string PrepareCommandline()
    {
        string xml = Settings.CustomBuildFile ?? UnrealBinaryBuilderHelpers.DEFAULT_BUILD_XML_FILE;
        string configs = string.Join(";", Settings.GameConfigurations);
        string args = $"BuildGraph -target=\"Make Installed Build Win64\" -script=\"{xml}\" " +
                     $"-set:WithDDC={GetBoolStr(Settings.bWithDDC)} " +
                     $"-set:SignExecutables={GetBoolStr(Settings.bSignExecutables)} " +
                     $"-set:EmbedSrcSrvInfo={GetBoolStr(Settings.bEnableSymStore)} " +
                     $"-set:GameConfigurations={configs} " +
                     $"-set:WithFullDebugInfo={GetBoolStr(Settings.bWithFullDebugInfo)} " +
                     $"-set:HostPlatformOnly={GetBoolStr(Settings.bHostPlatformOnly)} " +
                     $"-set:HostPlatformEditorOnly={GetBoolStr(Settings.bHostPlatformEditorOnly)} ";
        if (Settings.bWithDDC && Settings.bHostPlatformDDCOnly) args += "-set:HostPlatformDDCOnly=true ";
        if (Settings.bHostPlatformOnly) args += "-set:HostPlatformOnly=true ";
        else {
            if (SupportWin32) args += $"-set:WithWin32={GetBoolStr(Settings.bWithWin32)} ";
            args += $"-set:WithWin64={GetBoolStr(Settings.bWithWin64)} -set:WithMac={GetBoolStr(Settings.bWithMac)} -set:WithAndroid={GetBoolStr(Settings.bWithAndroid)} -set:WithIOS={GetBoolStr(Settings.bWithIOS)} -set:WithTVOS={GetBoolStr(Settings.bWithTVOS)} -set:WithLinux={GetBoolStr(Settings.bWithLinux)} -set:WithLumin={GetBoolStr(Settings.bWithLumin)} ";
            if (SupportHTML5) args += $"-set:WithHTML5={GetBoolStr(Settings.bWithHTML5)} ";
            if (SupportConsoles) args += $"-set:WithSwitch={GetBoolStr(Settings.bWithSwitch)} -set:WithPS4={GetBoolStr(Settings.bWithPS4)} -set:WithXboxOne={GetBoolStr(Settings.bWithXboxOne)} ";
            if (SupportLinuxArm64) args += $"-set:WithLinuxArm64={GetBoolStr(Settings.bWithLinuxAArch64)} ";
            else if (SupportLinuxAArch64) args += $"-set:WithLinuxAArch64={GetBoolStr(Settings.bWithLinuxAArch64)} ";
        }
        if (!string.IsNullOrEmpty(Settings.AnalyticsOverride)) args += $"-set:AnalyticsTypeOverride={Settings.AnalyticsOverride} ";
        if (SupportServerClientTargets) args += $"-set:WithServer={GetBoolStr(Settings.bWithServer)} -set:WithClient={GetBoolStr(Settings.bWithClient)} -set:WithHoloLens={GetBoolStr(Settings.bWithHoloLens)} ";
        if (IsEngineSelection425OrAbove) args += $"-set:CompileDatasmithPlugins={GetBoolStr(Settings.bCompileDatasmithPlugins)} ";
        if (Settings.bWithWin64NoPCH) args += "-set:WithWin64=true -set:BuildWithPrecompiledHeader=false ";
        if (SelectedVsVersion != null) args += $"-set:VS{SelectedVsVersion.Version}=true ";
        if (Settings.bCleanBuild) args += "-Clean ";
        if (!string.IsNullOrEmpty(Settings.CustomOptions)) args += $"{Settings.CustomOptions} ";
        return args;
    }

    private string GetBoolStr(bool b) => b.ToString().ToLower();

    public void AddLogEntry(string message, bool isError = false)
    {
        if (string.IsNullOrEmpty(message)) return;
        const string sp = @"\*{6} \[(\d+)\/(\d+)\]", pp = @"\w.+\.(cpp|cc|c|h|ispc)";
        if (Regex.IsMatch(message, sp)) { var m = Regex.Match(message, sp); _compiledFiles = 0; if (int.TryParse(m.Groups[2].Value, out int t)) _compiledFilesTotal = t; }
        if (Regex.IsMatch(message, pp)) { _compiledFiles++; CompiledFilesText = $"[Compiled: {_compiledFiles}/{_compiledFilesTotal}]"; }
        if (isError) _logMessageErrors += message + "\n";
        LogText += (isError ? "[ERROR] " : "") + message + "\n";
    }

    [RelayCommand]
    private void EditTargetFile(string type)
    {
        if (string.IsNullOrEmpty(EnginePath)) { ShowToast("Select Engine path first.", UBBNotificationType.Error); return; }
        string ue4 = $"UE4{type}.Target.cs", ue5 = $"Unreal{type}.Target.cs";
        string path = Path.Combine(EnginePath, "Engine", "Source", ue5);
        if (!File.Exists(path)) path = Path.Combine(EnginePath, "Engine", "Source", ue4);
        if (File.Exists(path)) {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                var editor = new CodeEditorWindow(); editor.LoadFile(path); editor.Show(desktop.MainWindow!);
            }
        } else ShowToast($"{path} does not exist.", UBBNotificationType.Error);
    }

    [RelayCommand]
    private void OpenBuildFolder()
    {
        if (string.IsNullOrEmpty(EnginePath)) return;
        string path = Path.Combine(EnginePath, "LocalBuilds", "Engine");
        if (Directory.Exists(path)) _platformService.OpenFolder(path);
        else ShowToast("Build folder does not exist yet.", UBBNotificationType.Info);
    }

    [RelayCommand] private void GetSourceCode() => _platformService.OpenUrl("https://github.com/EpicGames/UnrealEngine");
    [RelayCommand] private void OpenLogFolder() => _settingsService.OpenLogFolder();
    [RelayCommand] private void RemovePlugin(PluginCardViewModel p) => PluginQueue.Remove(p);
    [RelayCommand] private void OpenSupport() => _platformService.OpenUrl("https://github.com/ryanjon2040/Unreal-Binary-Builder");
    [RelayCommand] private void OpenChangelog() => _platformService.OpenUrl("https://github.com/ryanjon2040/Unreal-Binary-Builder/blob/master/CHANGELOG.md");
    [RelayCommand] private void OpenAbout() { if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d) { var dlg = new AboutDialog(); dlg.ShowDialog(d.MainWindow!); } }
    [RelayCommand] private void OpenSettings() => _settingsService.OpenSettings();

}
