using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnrealBinaryBuilder.Avalonia.Models;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class SettingsService : ISettingsService
{
    private readonly IPlatformService _platformService;
    private readonly string _programSavedPath;
    
    public static string DefaultSavedPath => PathHelpers.NormalizePath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "UnrealBinaryBuilder"));

    public string PROGRAM_SAVED_PATH => _programSavedPath;
    private string PROGRAM_SETTINGS_PATH_BASE => PathHelpers.ToUnixPath(Path.Combine(PROGRAM_SAVED_PATH, "Saved"));
    private string PROGRAM_SETTINGS_FILE_NAME => "Settings.json";
    private string PROGRAM_LOG_PATH_BASE => PathHelpers.ToUnixPath(Path.Combine(PROGRAM_SAVED_PATH, "Logs"));
    private string PROGRAM_LOG_FILE_NAME => "UnrealBinaryBuilder.log";
    private string PROGRAM_ERRORLOG_FILE_NAME => "BuildErrors.log";
    private string PROGRAM_SETTINGS_PATH => PathHelpers.ToUnixPath(Path.Combine(PROGRAM_SETTINGS_PATH_BASE, PROGRAM_SETTINGS_FILE_NAME));
    private string PROGRAM_LOG_PATH => PathHelpers.ToUnixPath(Path.Combine(PROGRAM_LOG_PATH_BASE, PROGRAM_LOG_FILE_NAME));
    private string PROGRAM_ERRORLOG_PATH => PathHelpers.ToUnixPath(Path.Combine(PROGRAM_LOG_PATH_BASE, PROGRAM_ERRORLOG_FILE_NAME));
    private string DEFAULT_GIT_CUSTOM_CACHE_PATH => PathHelpers.ToUnixPath(Path.Combine(PROGRAM_SAVED_PATH, "GitCache"));

    public event Action<string, LogMessageType>? OnLog;

    public SettingsService(IPlatformService platformService, string? programSavedPath = null)
    {
        _platformService = platformService;
        _programSavedPath = programSavedPath ?? DefaultSavedPath;
    }

    private void Log(string message, LogMessageType type = LogMessageType.Info) => OnLog?.Invoke(message, type);

    public BuilderSettingsJson GetSettings()
    {
        BuilderSettingsJson? ReturnValue = null;
        if (File.Exists(PROGRAM_SETTINGS_PATH))
        {
            try
            {
                string JsonOutput = File.ReadAllText(PROGRAM_SETTINGS_PATH);
                ReturnValue = JsonConvert.DeserializeObject<BuilderSettingsJson>(JsonOutput, [new Newtonsoft.Json.Converters.StringEnumConverter()]);
                if (ReturnValue != null)
                {
                    Log($"Settings loaded from {PROGRAM_SETTINGS_PATH}.", LogMessageType.Info);
                    return ReturnValue;
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to load settings: {ex.Message}", LogMessageType.Error);
            }
        }

        if (Directory.Exists(PROGRAM_SAVED_PATH) == false) Directory.CreateDirectory(PROGRAM_SAVED_PATH);
        if (Directory.Exists(PROGRAM_SETTINGS_PATH_BASE) == false) Directory.CreateDirectory(PROGRAM_SETTINGS_PATH_BASE);
        if (Directory.Exists(DEFAULT_GIT_CUSTOM_CACHE_PATH) == false) Directory.CreateDirectory(DEFAULT_GIT_CUSTOM_CACHE_PATH);

        ReturnValue = GenerateDefaultSettingsJSON();
        return ReturnValue;
    }

    private BuilderSettingsJson GenerateDefaultSettingsJSON()
    {
        var BSJ = GetDefaultSettings(_programSavedPath);
        string JsonOutput = JsonConvert.SerializeObject(BSJ, Formatting.Indented, [new Newtonsoft.Json.Converters.StringEnumConverter()]);
        File.WriteAllText(PROGRAM_SETTINGS_PATH, JsonOutput);
        Log($"New Settings file written to {PROGRAM_SETTINGS_PATH}.", LogMessageType.Info);
        return JsonConvert.DeserializeObject<BuilderSettingsJson>(JsonOutput, [new Newtonsoft.Json.Converters.StringEnumConverter()])!;
    }

    public void SaveSettings(BuilderSettingsJson settings)
    {
        string JsonOutput = JsonConvert.SerializeObject(settings, Formatting.Indented, [new Newtonsoft.Json.Converters.StringEnumConverter()]);
        File.WriteAllText(PROGRAM_SETTINGS_PATH, JsonOutput);
        Log($"Settings saved to {PROGRAM_SETTINGS_PATH}.", LogMessageType.Info);
    }

    public void WriteToLogFile(string InContent)
    {
        if (Directory.Exists(PROGRAM_LOG_PATH_BASE) == false) Directory.CreateDirectory(PROGRAM_LOG_PATH_BASE);
        File.WriteAllText(PROGRAM_LOG_PATH, InContent);
    }

    public void WriteErrorsToLogFile(string InContent)
    {
        try { File.Delete(PROGRAM_ERRORLOG_PATH); } catch (Exception) { }
        if (string.IsNullOrWhiteSpace(InContent)) return;
        if (Directory.Exists(PROGRAM_LOG_PATH_BASE) == false) Directory.CreateDirectory(PROGRAM_LOG_PATH_BASE);
        File.WriteAllText(PROGRAM_ERRORLOG_PATH, InContent);
    }

    public void OpenLogFolder()
    {
        if (Directory.Exists(PROGRAM_LOG_PATH_BASE))
        {
            _platformService.OpenFolder(PROGRAM_LOG_PATH_BASE);
        }
    }

    public void OpenSettings()
    {
        if (File.Exists(PROGRAM_SETTINGS_PATH))
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start("notepad.exe", PROGRAM_SETTINGS_PATH);
            }
            else
            {
                _platformService.OpenUrl(PROGRAM_SETTINGS_PATH);
            }
        }
    }

    public static BuilderSettingsJson GetDefaultSettings(string programSavedPath) => new()
    {
        Theme = "Dark",
        CheckForUpdatesAtStartup = true,
        EnableDDCMessages = true,
        EnableEngineBuildConfirmationMessage = true,
        ShowHTML5DeprecatedMessage = true,
        ShowConsoleDeprecatedMessage = true,
        SetupBatFile = null,
        CustomBuildFile = null,
        GameConfigurations = [BuildConfiguration.Development, BuildConfiguration.Shipping],
        CustomOptions = null,
        AnalyticsOverride = null,
        GitDependencyAll = true,
        GitDependencyPlatforms =
        [
            new ("Win64", true),
            new ("Win32", true),
            new ("Linux", false),
            new ("LinuxArm64", false),
            new ("Android", false),
            new ("Mac", false),
            new ("IOS", false),
            new ("TVOS", false),
            new ("HoloLens", false),
            new ("Lumin", false)
        ],
        GitDependencyThreads = 4,
        GitDependencyMaxRetries = 4,
        GitDependencyProxy = "",
        GitDependencyCache = PathHelpers.ToUnixPath(Path.Combine(programSavedPath, "GitCache")),
        GitDependencyCacheMultiplier = 2.0,
        GitDependencyCacheDays = 7,
        GitDependencyEnableCache = true,
        HostPlatformOnly = false,
        HostPlatformEditorOnly = false,
        WithWin64 = true,
        WithWin32 = true,
        WithMac = false,
        WithLinux = false,
        WithLinuxAArch64 = false,
        WithAndroid = false,
        WithIOS = false,
        WithHTML5 = false,
        WithTVOS = false,
        WithSwitch = false,
        WithPS4 = false,
        WithXboxOne = false,
        WithLumin = false,
        WithHoloLens = false,
        WithDDC = true,
        HostPlatformDDCOnly = true,
        SignExecutables = false,
        EnableSymStore = false,
        WithFullDebugInfo = false,
        CleanBuild = false,
        WithWin64NoPCH = false,
        WithServer = false,
        WithClient = false,
        CompileDatasmithPlugins = false,
        ShutdownPC = false,
        ShutdownIfBuildSuccess = false,
        ContinueToEngineBuild = true,
        BuildSetupBatFile = true,
        GenerateProjectFiles = true,
        BuildAutomationTool = true,
        ZipEngineBuild = false,
        ZipEngineDebug = false,
        ZipEngineDocumentation = true,
        ZipEngineExtras = true,
        ZipEngineFastCompression = true,
        ZipEngineFeaturePacks = true,
        ZipEnginePDB = true,
        ZipEngineSamples = true,
        ZipEngineSource = true,
        ZipEngineTemplates = true,
        ZipEnginePath = ""
    };
}
