using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UnrealBinaryBuilder.UserControls;

namespace UnrealBinaryBuilder.Classes
{
    public enum BuildConfiguration
    {
        Debug,
        DebugGame,
        Development,
        Shipping,
        Test
    }

	public class BuilderSettingsJson
	{
		// Application settings
		public string Theme { get; set; } // Valid settings are Dark, Light, Violet
		public bool bCheckForUpdatesAtStartup { get; set; }
		public bool bEnableDDCMessages { get; set; }
		public bool bEnableEngineBuildConfirmationMessage { get; set; }
		public bool bShowHTML5DeprecatedMessage { get; set; }
		public bool bShowConsoleDeprecatedMessage { get; set; }

		public string? SetupBatFile { get; set; }
		public string? CustomBuildFile { get; set; }
		public HashSet<BuildConfiguration> GameConfigurations { get; set; }
		public string? CustomOptions { get; set; }
		public string? AnalyticsOverride { get; set; }

		public bool GitDependencyAll { get; set; }
		public List<GitPlatform> GitDependencyPlatforms { get; set; }
		public int GitDependencyThreads { get; set; }
		public int GitDependencyMaxRetries { get; set; }
		public string GitDependencyProxy { get; set; }
		public bool GitDependencyEnableCache { get; set; }
		public string GitDependencyCache { get; set; }
		public double GitDependencyCacheMultiplier { get; set; }
		public int GitDependencyCacheDays { get; set; }

		public bool bHostPlatformOnly { get; set; }
		public bool bHostPlatformEditorOnly { get; set; }
		public bool bWithWin64 { get; set; }
		public bool bWithWin32 { get; set; }
		public bool bWithMac { get; set; }
		public bool bWithLinux { get; set; }
		public bool bWithLinuxAArch64 { get; set; }
		public bool bWithAndroid { get; set; }
		public bool bWithIOS { get; set; }
		public bool bWithHTML5 { get; set; }
		public bool bWithTVOS { get; set; }
		public bool bWithSwitch { get; set; }
		public bool bWithPS4 { get; set; }
		public bool bWithXboxOne { get; set; }
		public bool bWithLumin { get; set; }
		public bool bWithHoloLens { get; set; }

		public bool bWithDDC { get; set; }
		public bool bHostPlatformDDCOnly { get; set; }
		public bool bSignExecutables { get; set; }
		public bool bEnableSymStore { get; set; }
		public bool bWithFullDebugInfo { get; set; }
		public bool bCleanBuild { get; set; }
        public bool bWithWin64NoPCH { get; set; }
        public bool bWithServer { get; set; }
		public bool bWithClient { get; set; }
		public bool bCompileDatasmithPlugins { get; set; }
		//public bool bVS2019 { get; set; }
		public bool bShutdownPC { get; set; }
		public bool bShutdownIfBuildSuccess { get; set; }
		public bool bContinueToEngineBuild { get; set; }
		public bool bBuildSetupBatFile { get; set; }
		public bool bGenerateProjectFiles { get; set; }
		public bool bBuildAutomationTool { get; set; }

		public bool bZipEngineBuild { get; set; }		
		public bool bZipEnginePDB { get; set; }
		public bool bZipEngineDebug { get; set; }
		public bool bZipEngineDocumentation { get; set; }
		public bool bZipEngineExtras { get; set; }
		public bool bZipEngineSource { get; set; }
		public bool bZipEngineFeaturePacks { get; set; }
		public bool bZipEngineSamples { get; set; }
		public bool bZipEngineTemplates { get; set; }
		public bool bZipEngineFastCompression { get; set; }
		public string ZipEnginePath { get; set; }

		public VisualStudio VisualStudio { get; set; }
    }

    public class VisualStudio
    { 
        public VisualStudio(int version, string edition, string architecture)
        {
            Version = version;
            Edition = edition;
            Architecture = architecture;
        }

        public int Version { get; set; }
        public string Edition { get; set; }
        public string Architecture { get; set; }
    }

	public class GitPlatform
	{
		public GitPlatform(string InName, bool bInclude)
		{
			Name = InName;
			bIsIncluded = bInclude;
		}

		public string Name { get; set; }
		public bool bIsIncluded { get; set; }
	}

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

        private static readonly BuilderSettingsJson DEFAULT_SETTINGS = new BuilderSettingsJson
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
                //bVS2019 = false,
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

		private static MainWindow Window => (MainWindow)Application.Current.MainWindow;

        private static BuilderSettingsJson GenerateDefaultSettingsJSON()
        {
            var BSJ = DefaultSettings;

            string JsonOutput = JsonConvert.SerializeObject(BSJ, Formatting.Indented, [new Newtonsoft.Json.Converters.StringEnumConverter()]);
			File.WriteAllText(PROGRAM_SETTINGS_PATH, JsonOutput);
			LogEntry logEntry = new()
            {
                Message = $"New Settings file written to {PROGRAM_SETTINGS_PATH}."
            };
            Window.LogControl.AddLogEntry(logEntry, LogViewer.EMessageType.Info);
			Window.OpenSettingsBtn.IsEnabled = true;
			return JsonConvert.DeserializeObject<BuilderSettingsJson>(JsonOutput, [new Newtonsoft.Json.Converters.StringEnumConverter()])!;
		}

		public static BuilderSettingsJson GetSettingsFile(bool bLog = false)
		{
			if (Directory.Exists(PROGRAM_LOG_PATH_BASE))
			{
				Window.OpenLogFolderBtn.IsEnabled = true;
			}

			BuilderSettingsJson? ReturnValue = null;
			if (File.Exists(PROGRAM_SETTINGS_PATH))
			{
				string JsonOutput = File.ReadAllText(PROGRAM_SETTINGS_PATH);
				ReturnValue = JsonConvert.DeserializeObject<BuilderSettingsJson>(JsonOutput, [new Newtonsoft.Json.Converters.StringEnumConverter()]);
                if (ReturnValue != null)
                {
                    if (bLog)
                    {
                        LogEntry logEntry = new LogEntry
                        {
                            Message = $"Settings loaded from {PROGRAM_SETTINGS_PATH}."
                        };
                        Window.LogControl.AddLogEntry(logEntry, LogViewer.EMessageType.Info);
                        Window.OpenSettingsBtn.IsEnabled = true;
                    }
                    return ReturnValue;
                }
            }

			if (Directory.Exists(PROGRAM_SAVED_PATH) == false)
			{
				Directory.CreateDirectory(PROGRAM_SAVED_PATH);
				if (bLog)
				{
					LogEntry logEntry = new LogEntry
                    {
                        Message = $"Directory created: {PROGRAM_SAVED_PATH}."
                    };
                    Window.LogControl.AddLogEntry(logEntry, LogViewer.EMessageType.Info);
				}
			}

			if (Directory.Exists(PROGRAM_SETTINGS_PATH_BASE) == false)
			{
				Directory.CreateDirectory(PROGRAM_SETTINGS_PATH_BASE);
				if (bLog)
				{
					LogEntry logEntry = new LogEntry
                    {
                        Message = $"Directory created: {PROGRAM_SETTINGS_PATH_BASE}."
                    };
                    Window.LogControl.AddLogEntry(logEntry, LogViewer.EMessageType.Info);
				}
			}

			if (Directory.Exists(DEFAULT_GIT_CUSTOM_CACHE_PATH) == false)
			{
				Directory.CreateDirectory(DEFAULT_GIT_CUSTOM_CACHE_PATH);
			}

			ReturnValue = GenerateDefaultSettingsJSON();

			return ReturnValue;
		}

		public static void SaveSettings()
		{
			MainWindow mainWindow = Window;
            var defaultSettings = GenerateDefaultSettingsJSON();

            BuilderSettingsJson BSJ = new()
            {
                Theme = mainWindow.CurrentTheme,
                bCheckForUpdatesAtStartup = mainWindow.context.SettingsJSON.bCheckForUpdatesAtStartup,
                SetupBatFile = mainWindow.SetupBatFilePath.Text,
                CustomBuildFile = mainWindow.CustomBuildXMLFile.Text,
                GameConfigurations = mainWindow.context.SettingsJSON.GameConfigurations,
                CustomOptions = mainWindow.CustomOptions.Text,
                AnalyticsOverride = mainWindow.AnalyticsOverride.Text,
                GitDependencyAll = mainWindow.bGitSyncAll.IsChecked ?? defaultSettings.GitDependencyAll,
                GitDependencyThreads = Convert.ToInt32(mainWindow.GitNumberOfThreads.Text),
                GitDependencyMaxRetries = Convert.ToInt32(mainWindow.GitNumberOfRetries.Text),
                GitDependencyProxy = "",
                GitDependencyCache = mainWindow.GitCachePath.Text,
                GitDependencyCacheMultiplier = Convert.ToDouble(mainWindow.GitCacheMultiplier.Text),
                GitDependencyCacheDays = Convert.ToInt32(mainWindow.GitCacheDays.Text),
                GitDependencyEnableCache = mainWindow.bGitEnableCache.IsChecked ?? defaultSettings.GitDependencyEnableCache,
                bHostPlatformOnly = mainWindow.bHostPlatformOnly.IsChecked ?? defaultSettings.bHostPlatformOnly,
                bHostPlatformEditorOnly = mainWindow.bHostPlatformEditorOnly.IsChecked ?? defaultSettings.bHostPlatformEditorOnly,
                bWithWin64 = mainWindow.bWithWin64.IsChecked ?? defaultSettings.bWithWin64,
                bWithWin32 = mainWindow.bWithWin32.IsChecked ?? defaultSettings.bWithWin32,
                bWithMac = mainWindow.bWithMac.IsChecked ?? defaultSettings.bWithMac,
                bWithLinux = mainWindow.bWithLinux.IsChecked ?? defaultSettings.bWithLinux,
                bWithLinuxAArch64 = mainWindow.bWithLinuxAArch64.IsChecked ?? defaultSettings.bWithLinuxAArch64,
                bWithAndroid = mainWindow.bWithAndroid.IsChecked ?? defaultSettings.bWithAndroid,
                bWithIOS = mainWindow.bWithIOS.IsChecked ?? defaultSettings.bWithIOS,
                bWithHTML5 = mainWindow.bWithHTML5.IsChecked ?? defaultSettings.bWithHTML5,
                bWithTVOS = mainWindow.bWithTVOS.IsChecked ?? defaultSettings.bWithTVOS,
                bWithSwitch = mainWindow.bWithSwitch.IsChecked ?? defaultSettings.bWithSwitch,
                bWithPS4 = mainWindow.bWithPS4.IsChecked ?? defaultSettings.bWithPS4,
                bWithXboxOne = mainWindow.bWithXboxOne.IsChecked ?? defaultSettings.bWithXboxOne,
                bWithLumin = mainWindow.bWithLumin.IsChecked ?? defaultSettings.bWithLumin,
                bWithHoloLens = mainWindow.bWithHololens.IsChecked ?? defaultSettings.bWithHoloLens,
                bWithDDC = mainWindow.bWithDDC.IsChecked ?? defaultSettings.bWithDDC,
                bHostPlatformDDCOnly = mainWindow.bHostPlatformDDCOnly.IsChecked ?? defaultSettings.bHostPlatformDDCOnly,
                bSignExecutables = mainWindow.bSignExecutables.IsChecked ?? defaultSettings.bSignExecutables,
                bEnableSymStore = mainWindow.bEnableSymStore.IsChecked ?? defaultSettings.bEnableSymStore,
                bWithFullDebugInfo = mainWindow.bWithFullDebugInfo.IsChecked ?? defaultSettings.bWithFullDebugInfo,
                bCleanBuild = mainWindow.bCleanBuild.IsChecked ?? defaultSettings.bCleanBuild,
                bWithWin64NoPCH = mainWindow.bWithWin64NoPCH.IsChecked ?? defaultSettings.bWithWin64NoPCH,
                bWithServer = mainWindow.bWithServer.IsChecked ?? defaultSettings.bWithServer,
                bWithClient = mainWindow.bWithClient.IsChecked ?? defaultSettings.bWithClient,
                bCompileDatasmithPlugins = mainWindow.bCompileDatasmithPlugins.IsChecked ?? defaultSettings.bCompileDatasmithPlugins,
				//VisualStudio = (VisualStudio)mainWindow.,
                bShutdownPC = mainWindow.bShutdownWindows.IsChecked ?? defaultSettings.bShutdownPC,
                bShutdownIfBuildSuccess = mainWindow.bShutdownIfSuccess.IsChecked ?? defaultSettings.bShutdownIfBuildSuccess,
                bContinueToEngineBuild = mainWindow.bContinueToEngineBuild.IsChecked ?? defaultSettings.bContinueToEngineBuild,
                bBuildSetupBatFile = mainWindow.bBuildSetupBatFile.IsChecked ?? defaultSettings.bBuildSetupBatFile,
                bGenerateProjectFiles = mainWindow.bGenerateProjectFiles.IsChecked ?? defaultSettings.bGenerateProjectFiles,
                bBuildAutomationTool = mainWindow.bBuildAutomationTool.IsChecked ?? defaultSettings.bBuildAutomationTool,
                bZipEngineBuild = mainWindow.bZipBuild.IsChecked ?? defaultSettings.bZipEngineBuild,
                bZipEngineDebug = mainWindow.bIncludeDEBUG.IsChecked ?? defaultSettings.bZipEngineDebug,
                bZipEngineDocumentation = mainWindow.bIncludeDocumentation.IsChecked ?? defaultSettings.bZipEngineDocumentation,
                bZipEngineExtras = mainWindow.bIncludeExtras.IsChecked ?? defaultSettings.bZipEngineExtras,
                bZipEngineFastCompression = mainWindow.bFastCompression.IsChecked ?? defaultSettings.bZipEngineFastCompression,
                bZipEngineFeaturePacks = mainWindow.bIncludeFeaturePacks.IsChecked ?? defaultSettings.bZipEngineFeaturePacks,
                bZipEnginePDB = mainWindow.bIncludePDB.IsChecked ?? defaultSettings.bZipEnginePDB,
                bZipEngineSamples = mainWindow.bIncludeSamples.IsChecked ?? defaultSettings.bZipEngineSamples,
                bZipEngineSource = mainWindow.bIncludeSource.IsChecked ?? defaultSettings.bZipEngineSource,
                bZipEngineTemplates = mainWindow.bIncludeTemplates.IsChecked ?? defaultSettings.bZipEngineTemplates,
                ZipEnginePath = mainWindow.ZipPath.Text
            };

            BSJ.GameConfigurations = mainWindow.context.SettingsJSON.GameConfigurations;
            /*
            var allBuildConfig = Enum.GetValues(typeof(BuildConfiguration)).Cast<BuildConfiguration>().ToArray();
            foreach (var config in allBuildConfig)
            {
                string CheckboxName = $"GameConfig{Enum.GetName(config.GetType(), config)}";
                bool? check_val = ((CheckBox?)mainWindow.FindName(CheckboxName))?.IsChecked;
                if (check_val == null)
                {
                    check_val = defaultSettings.GameConfigurations.Contains(config);
                }

            }
            */
            /*var gameConfigurations = mainWindow.context.SettingsJSON.GameConfigurations;
            foreach (var config in gameConfigurations)
            {
                string CheckboxName = $"GameConfig{Enum.GetName(config.GetType(), config)}";
                ((CheckBox)mainWindow.FindName(CheckboxName)!).IsChecked;
            }*/

            List<GitPlatform> GitPlatformList = mainWindow.context.SettingsJSON.GitDependencyPlatforms;
			IEnumerable<CheckBox> ComboBoxCollection = GetChildrenOfType<CheckBox>(mainWindow.PlatformStackPanelMain).ToArray();
			foreach (GitPlatform gp in GitPlatformList)
			{
				string ComboBoxName = $"Git{gp.Name}Platform";
				foreach (CheckBox c in ComboBoxCollection)
                {
                    if (!string.Equals(c.Name, ComboBoxName, StringComparison.CurrentCultureIgnoreCase)) 
                        continue;

                    gp.bIsIncluded = c.IsChecked ?? false;
                    break;
                }
			}
			BSJ.GitDependencyPlatforms = GitPlatformList;

			string JsonOutput = JsonConvert.SerializeObject(BSJ, Formatting.Indented, [new Newtonsoft.Json.Converters.StringEnumConverter()]);
			File.WriteAllText(PROGRAM_SETTINGS_PATH, JsonOutput);
			LogEntry logEntry = new()
            {
                Message = $"New Settings file written to {PROGRAM_SETTINGS_PATH}."
            };
            mainWindow.LogControl.AddLogEntry(logEntry, LogViewer.EMessageType.Info);
		}

		public static void WriteToLogFile(string InContent)
		{
			if (Directory.Exists(PROGRAM_LOG_PATH_BASE) == false)
			{
				Directory.CreateDirectory(PROGRAM_LOG_PATH_BASE);
				MainWindow mainWindow = Window;
				mainWindow.OpenLogFolderBtn.IsEnabled = true;
			}
			File.WriteAllText(PROGRAM_LOG_PATH, InContent);
		}

		public static void WriteErrorsToLogFile(string InContent)
		{
			try
			{
				File.Delete(PROGRAM_ERRORLOG_PATH);
			}
			catch (Exception) {}

            if (string.IsNullOrWhiteSpace(InContent)) return;

            if (Directory.Exists(PROGRAM_LOG_PATH_BASE) == false)
            {
                Directory.CreateDirectory(PROGRAM_LOG_PATH_BASE);
            }
            File.WriteAllText(PROGRAM_ERRORLOG_PATH, InContent);
        }

		public static void UpdatePlatformInclusion(string InPlatform, bool bIncluded)
		{
			try
			{
				BuilderSettingsJson BSJ = GetSettingsFile();
				foreach (var gp in BSJ.GitDependencyPlatforms.Where(gp => string.Equals(gp.Name, InPlatform, StringComparison.CurrentCultureIgnoreCase)))
                {
                    gp.bIsIncluded = bIncluded;
                    break;
                }

				string JsonOutput = JsonConvert.SerializeObject(BSJ, Formatting.Indented, [new Newtonsoft.Json.Converters.StringEnumConverter()]);
				File.WriteAllText(PROGRAM_SETTINGS_PATH, JsonOutput);
			}
			catch (Exception ex)
			{
				string ErrorMessage = $"Failed to update platform setting. ERROR: {ex.Message}";
				GameAnalyticsCSharp.LogEvent(ErrorMessage, GameAnalyticsSDK.Net.EGAErrorSeverity.Error);
				MainWindow mainWindow = Window;
				mainWindow.AddLogEntry(ErrorMessage, true);
				HandyControl.Controls.MessageBox.Fatal(ErrorMessage);
			}
		}

		public static void LoadInitialValues()
		{
			MainWindow mainWindow = Window;

            var allBuildConfig = Enum.GetValues(typeof(BuildConfiguration)).Cast<BuildConfiguration>().ToArray();
            foreach (var config in allBuildConfig)
            {
                string CheckboxName = $"GameConfig{Enum.GetName(config.GetType(), config)}";
                var checkbox = ((CheckBox?)mainWindow.FindName(CheckboxName));
                if (checkbox == null) continue;

                checkbox.IsChecked = mainWindow.context.SettingsJSON.GameConfigurations.Contains(config);
            }

            List<GitPlatform> GitPlatformList = mainWindow.context.SettingsJSON.GitDependencyPlatforms;
			IEnumerable<CheckBox> ComboBoxCollection = GetChildrenOfType<CheckBox>(mainWindow.PlatformStackPanelMain).ToArray();

			foreach (GitPlatform gp in GitPlatformList)
			{
				string ComboBoxName = $"Git{gp.Name}Platform";
				foreach (CheckBox c in ComboBoxCollection)
                {
                    if (!string.Equals(c.Name, ComboBoxName, StringComparison.CurrentCultureIgnoreCase)) 
                        continue;

                    c.IsChecked = gp.bIsIncluded;
                    break;
                }
			}
		}

		public static void OpenLogFolder()
		{
			if (Directory.Exists(PROGRAM_LOG_PATH_BASE))
			{
				Process.Start("explorer.exe", PROGRAM_LOG_PATH_BASE);
			}			
		}

		public static void OpenSettings()
		{
			if (File.Exists(PROGRAM_SETTINGS_PATH))
			{
				Process.Start("notepad.exe", PROGRAM_SETTINGS_PATH);
			}
		}

		public static IEnumerable<T> GetChildrenOfType<T>(DependencyObject? dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null) 
                yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child != null && child is T)
                {
                    yield return (T)child;
                }

                foreach (T childOfChild in GetChildrenOfType<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }
	}
}
