using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public enum LogMessageType { Info, Warning, Error }

public static class BuilderSettings
{
    private static readonly string PROGRAM_SAVED_PATH_BASE = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public static readonly string PROGRAM_SAVED_PATH = Path.Combine(PROGRAM_SAVED_PATH_BASE, "UnrealBinaryBuilder");

    private static readonly string PROGRAM_SETTINGS_PATH_BASE = Path.Combine(PROGRAM_SAVED_PATH, "Saved");
    private static readonly string PROGRAM_SETTINGS_FILE_NAME = "Settings.json";

    private static readonly string PROGRAM_LOG_PATH_BASE = Path.Combine(PROGRAM_SAVED_PATH, "Logs");
    private static readonly string PROGRAM_LOG_FILE_NAME = "UnrealBinaryBuilder.log";
    private static readonly string PROGRAM_ERRORLOG_FILE_NAME = "BuildErrors.log";

    private static readonly string PROGRAM_SETTINGS_PATH = Path.Combine(PROGRAM_SETTINGS_PATH_BASE, PROGRAM_SETTINGS_FILE_NAME);
    private static readonly string PROGRAM_LOG_PATH = Path.Combine(PROGRAM_LOG_PATH_BASE, PROGRAM_LOG_FILE_NAME);
    private static readonly string PROGRAM_ERRORLOG_PATH = Path.Combine(PROGRAM_LOG_PATH_BASE, PROGRAM_ERRORLOG_FILE_NAME);

    private static readonly string DEFAULT_GIT_CUSTOM_CACHE_PATH = Path.Combine(PROGRAM_SAVED_PATH, "GitCache");

    private static readonly BuilderSettingsJson DEFAULT_SETTINGS = new()
    {
        Theme = "Dark",
        bCheckForUpdatesAtStartup = true,
        bEnableDDCMessages = true,
        bEnableEngineBuildConfirmationMessage = true,
        bShowHTML5DeprecatedMessage = true,
        bShowConsoleDeprecatedMessage = true,
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
        GitDependencyCache = DEFAULT_GIT_CUSTOM_CACHE_PATH,
        GitDependencyCacheMultiplier = 2.0,
        GitDependencyCacheDays = 7,
        GitDependencyEnableCache = true,
        bHostPlatformOnly = false,
        bHostPlatformEditorOnly = false,
        bWithWin64 = true,
        bWithWin32 = true,
        bWithMac = false,
        bWithLinux = false,
        bWithLinuxAArch64 = false,
        bWithAndroid = false,
        bWithIOS = false,
        bWithHTML5 = false,
        bWithTVOS = false,
        bWithSwitch = false,
        bWithPS4 = false,
        bWithXboxOne = false,
        bWithLumin = false,
        bWithHoloLens = false,
        bWithDDC = true,
        bHostPlatformDDCOnly = true,
        bSignExecutables = false,
        bEnableSymStore = false,
        bWithFullDebugInfo = false,
        bCleanBuild = false,
        bWithWin64NoPCH = false,
        bWithServer = false,
        bWithClient = false,
        bCompileDatasmithPlugins = false,
        bShutdownPC = false,
        bShutdownIfBuildSuccess = false,
        bContinueToEngineBuild = true,
        bBuildSetupBatFile = true,
        bGenerateProjectFiles = true,
        bBuildAutomationTool = true,
        bZipEngineBuild = false,
        bZipEngineDebug = false,
        bZipEngineDocumentation = true,
        bZipEngineExtras = true,
        bZipEngineFastCompression = true,
        bZipEngineFeaturePacks = true,
        bZipEnginePDB = true,
        bZipEngineSamples = true,
        bZipEngineSource = true,
        bZipEngineTemplates = true,
        ZipEnginePath = ""
    };

    public static BuilderSettingsJson DefaultSettings => DEFAULT_SETTINGS;

    public static event Action<string, LogMessageType>? OnLog;

    private static void Log(string message, LogMessageType type = LogMessageType.Info) => OnLog?.Invoke(message, type);

    private static BuilderSettingsJson GenerateDefaultSettingsJSON()
    {
        var BSJ = DefaultSettings;
        string JsonOutput = JsonConvert.SerializeObject(BSJ, Formatting.Indented, [new Newtonsoft.Json.Converters.StringEnumConverter()]);
        File.WriteAllText(PROGRAM_SETTINGS_PATH, JsonOutput);
        Log($"New Settings file written to {PROGRAM_SETTINGS_PATH}.", LogMessageType.Info);
        return JsonConvert.DeserializeObject<BuilderSettingsJson>(JsonOutput, [new Newtonsoft.Json.Converters.StringEnumConverter()])!;
    }

    public static BuilderSettingsJson GetSettingsFile(bool bLog = false)
    {
        BuilderSettingsJson? ReturnValue = null;
        if (File.Exists(PROGRAM_SETTINGS_PATH))
        {
            string JsonOutput = File.ReadAllText(PROGRAM_SETTINGS_PATH);
            ReturnValue = JsonConvert.DeserializeObject<BuilderSettingsJson>(JsonOutput, [new Newtonsoft.Json.Converters.StringEnumConverter()]);
            if (ReturnValue != null)
            {
                if (bLog) Log($"Settings loaded from {PROGRAM_SETTINGS_PATH}.", LogMessageType.Info);
                return ReturnValue;
            }
        }

        if (Directory.Exists(PROGRAM_SAVED_PATH) == false) Directory.CreateDirectory(PROGRAM_SAVED_PATH);
        if (Directory.Exists(PROGRAM_SETTINGS_PATH_BASE) == false) Directory.CreateDirectory(PROGRAM_SETTINGS_PATH_BASE);
        if (Directory.Exists(DEFAULT_GIT_CUSTOM_CACHE_PATH) == false) Directory.CreateDirectory(DEFAULT_GIT_CUSTOM_CACHE_PATH);

        ReturnValue = GenerateDefaultSettingsJSON();
        return ReturnValue;
    }

    public static void SaveSettings(BuilderSettingsJson settings)
    {
        string JsonOutput = JsonConvert.SerializeObject(settings, Formatting.Indented, [new Newtonsoft.Json.Converters.StringEnumConverter()]);
        File.WriteAllText(PROGRAM_SETTINGS_PATH, JsonOutput);
        Log($"Settings saved to {PROGRAM_SETTINGS_PATH}.", LogMessageType.Info);
    }

    public static void WriteToLogFile(string InContent)
    {
        if (Directory.Exists(PROGRAM_LOG_PATH_BASE) == false) Directory.CreateDirectory(PROGRAM_LOG_PATH_BASE);
        File.WriteAllText(PROGRAM_LOG_PATH, InContent);
    }

    public static void WriteErrorsToLogFile(string InContent)
    {
        try { File.Delete(PROGRAM_ERRORLOG_PATH); } catch (Exception) { }
        if (string.IsNullOrWhiteSpace(InContent)) return;
        if (Directory.Exists(PROGRAM_LOG_PATH_BASE) == false) Directory.CreateDirectory(PROGRAM_LOG_PATH_BASE);
        File.WriteAllText(PROGRAM_ERRORLOG_PATH, InContent);
    }

    public static void OpenLogFolder()
    {
        if (Directory.Exists(PROGRAM_LOG_PATH_BASE))
        {
            if (OperatingSystem.IsWindows()) Process.Start("explorer.exe", PROGRAM_LOG_PATH_BASE);
            else if (OperatingSystem.IsLinux()) Process.Start("xdg-open", PROGRAM_LOG_PATH_BASE);
            else if (OperatingSystem.IsMacOS()) Process.Start("open", PROGRAM_LOG_PATH_BASE);
        }
    }

    public static void OpenSettings()
    {
        if (File.Exists(PROGRAM_SETTINGS_PATH))
        {
            if (OperatingSystem.IsWindows()) Process.Start("notepad.exe", PROGRAM_SETTINGS_PATH);
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()) Process.Start(new ProcessStartInfo(PROGRAM_SETTINGS_PATH) { UseShellExecute = true });
        }
    }
}
