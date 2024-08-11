using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UnrealBinaryBuilder.UserControls;
using System.Diagnostics;
using System.Linq;

namespace UnrealBinaryBuilder.Classes
{
	public class BuilderSettingsJson
	{
		// Application settings
		public string Theme { get; set; } // Valid settings are Dark, Light, Violet
		public bool bCheckForUpdatesAtStartup { get; set; }
		public bool bEnableDDCMessages { get; set; }
		public bool bEnableEngineBuildConfirmationMessage { get; set; }
		public bool bShowHTML5DeprecatedMessage { get; set; }
		public bool bShowConsoleDeprecatedMessage { get; set; }

		public string SetupBatFile { get; set; }
		public string CustomBuildFile { get; set; }
		public string GameConfigurations { get; set; }
		public string CustomOptions { get; set; }
		public string AnalyticsOverride { get; set; }

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

		private static MainWindow Window => (MainWindow)Application.Current.MainWindow;

        private static BuilderSettingsJson GenerateDefaultSettingsJSON()
		{
			BuilderSettingsJson BSJ = new()
            {
                Theme = "Dark",
                bCheckForUpdatesAtStartup = true,
                bEnableDDCMessages = true,
                bEnableEngineBuildConfirmationMessage = true,
                bShowHTML5DeprecatedMessage = true,
                bShowConsoleDeprecatedMessage = true,
                SetupBatFile = null,
                CustomBuildFile = null,
                GameConfigurations = "Development;Shipping",
                CustomOptions = null,
                AnalyticsOverride = null,
                GitDependencyAll = true,
                GitDependencyPlatforms = new List<GitPlatform> { 
                    new("Win64", true), 
                    new("Win32", true),
                    new("Linux", false),
                    new("Android", false),
                    new("Mac", false), 
                    new("IOS", false), 
                    new("TVOS", false),
                    new("HoloLens", false), 
                    new("Lumin", false) },
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

            string JsonOutput = JsonConvert.SerializeObject(BSJ, Formatting.Indented);
			File.WriteAllText(PROGRAM_SETTINGS_PATH, JsonOutput);
			LogEntry logEntry = new()
            {
                Message = $"New Settings file written to {PROGRAM_SETTINGS_PATH}."
            };
            Window.LogControl.AddLogEntry(logEntry, LogViewer.EMessageType.Info);
			Window.OpenSettingsBtn.IsEnabled = true;
			return JsonConvert.DeserializeObject<BuilderSettingsJson>(JsonOutput);
		}

		public static BuilderSettingsJson GetSettingsFile(bool bLog = false)
		{
			if (Directory.Exists(PROGRAM_LOG_PATH_BASE))
			{
				Window.OpenLogFolderBtn.IsEnabled = true;
			}

			BuilderSettingsJson ReturnValue = null;
			if (File.Exists(PROGRAM_SETTINGS_PATH))
			{
				string JsonOutput = File.ReadAllText(PROGRAM_SETTINGS_PATH);
				ReturnValue = JsonConvert.DeserializeObject<BuilderSettingsJson>(JsonOutput);
				if (bLog)
				{
					LogEntry logEntry = new LogEntry
                    {
                        Message = $"Settings loaded from {PROGRAM_SETTINGS_PATH}."
                    };
                    Window.LogControl.AddLogEntry(logEntry, LogViewer.EMessageType.Info);
					Window.OpenSettingsBtn.IsEnabled = true;
				}
			}
			else
			{
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
			}
			return ReturnValue;
		}

		public static void SaveSettings()
		{
			MainWindow mainWindow = Window;
			BuilderSettingsJson BSJ = new()
            {
                Theme = mainWindow.CurrentTheme,
                bCheckForUpdatesAtStartup = mainWindow.context.SettingsJSON.bCheckForUpdatesAtStartup,
                SetupBatFile = mainWindow.SetupBatFilePath.Text,
                CustomBuildFile = mainWindow.CustomBuildXMLFile.Text,
                GameConfigurations = mainWindow.GameConfigurations.Text,
                CustomOptions = mainWindow.CustomOptions.Text,
                AnalyticsOverride = mainWindow.AnalyticsOverride.Text,
                GitDependencyAll = (bool)mainWindow.bGitSyncAll.IsChecked,
                GitDependencyThreads = Convert.ToInt32(mainWindow.GitNumberOfThreads.Text),
                GitDependencyMaxRetries = Convert.ToInt32(mainWindow.GitNumberOfRetries.Text),
                GitDependencyProxy = "",
                GitDependencyCache = mainWindow.GitCachePath.Text,
                GitDependencyCacheMultiplier = Convert.ToDouble(mainWindow.GitCacheMultiplier.Text),
                GitDependencyCacheDays = Convert.ToInt32(mainWindow.GitCacheDays.Text),
                GitDependencyEnableCache = (bool)mainWindow.bGitEnableCache.IsChecked,
                bHostPlatformOnly = (bool)mainWindow.bHostPlatformOnly.IsChecked,
                bHostPlatformEditorOnly = (bool)mainWindow.bHostPlatformEditorOnly.IsChecked,
                bWithWin64 = (bool)mainWindow.bWithWin64.IsChecked,
                bWithWin32 = (bool)mainWindow.bWithWin32.IsChecked,
                bWithMac = (bool)mainWindow.bWithMac.IsChecked,
                bWithLinux = (bool)mainWindow.bWithLinux.IsChecked,
                bWithLinuxAArch64 = (bool)mainWindow.bWithLinuxAArch64.IsChecked,
                bWithAndroid = (bool)mainWindow.bWithAndroid.IsChecked,
                bWithIOS = (bool)mainWindow.bWithIOS.IsChecked,
                bWithHTML5 = (bool)mainWindow.bWithHTML5.IsChecked,
                bWithTVOS = (bool)mainWindow.bWithTVOS.IsChecked,
                bWithSwitch = (bool)mainWindow.bWithSwitch.IsChecked,
                bWithPS4 = (bool)mainWindow.bWithPS4.IsChecked,
                bWithXboxOne = (bool)mainWindow.bWithXboxOne.IsChecked,
                bWithLumin = (bool)mainWindow.bWithLumin.IsChecked,
                bWithHoloLens = (bool)mainWindow.bWithHololens.IsChecked,
                bWithDDC = (bool)mainWindow.bWithDDC.IsChecked,
                bHostPlatformDDCOnly = (bool)mainWindow.bHostPlatformDDCOnly.IsChecked,
                bSignExecutables = (bool)mainWindow.bSignExecutables.IsChecked,
                bEnableSymStore = (bool)mainWindow.bEnableSymStore.IsChecked,
                bWithFullDebugInfo = (bool)mainWindow.bWithFullDebugInfo.IsChecked,
                bCleanBuild = (bool)mainWindow.bCleanBuild.IsChecked,
                bWithServer = (bool)mainWindow.bWithServer.IsChecked,
                bWithClient = (bool)mainWindow.bWithClient.IsChecked,
                bCompileDatasmithPlugins = (bool)mainWindow.bCompileDatasmithPlugins.IsChecked,
                //bVS2019 = (bool)mainWindow.bVS2019.IsChecked,
				//VisualStudio = (VisualStudio)mainWindow.,
                bShutdownPC = (bool)mainWindow.bShutdownWindows.IsChecked,
                bShutdownIfBuildSuccess = (bool)mainWindow.bShutdownIfSuccess.IsChecked,
                bContinueToEngineBuild = (bool)mainWindow.bContinueToEngineBuild.IsChecked,
                bBuildSetupBatFile = (bool)mainWindow.bBuildSetupBatFile.IsChecked,
                bGenerateProjectFiles = (bool)mainWindow.bGenerateProjectFiles.IsChecked,
                bBuildAutomationTool = (bool)mainWindow.bBuildAutomationTool.IsChecked,
                bZipEngineBuild = (bool)mainWindow.bZipBuild.IsChecked,
                bZipEngineDebug = (bool)mainWindow.bIncludeDEBUG.IsChecked,
                bZipEngineDocumentation = (bool)mainWindow.bIncludeDocumentation.IsChecked,
                bZipEngineExtras = (bool)mainWindow.bIncludeExtras.IsChecked,
                bZipEngineFastCompression = (bool)mainWindow.bFastCompression.IsChecked,
                bZipEngineFeaturePacks = (bool)mainWindow.bIncludeFeaturePacks.IsChecked,
                bZipEnginePDB = (bool)mainWindow.bIncludePDB.IsChecked,
                bZipEngineSamples = (bool)mainWindow.bIncludeSamples.IsChecked,
                bZipEngineSource = (bool)mainWindow.bIncludeSource.IsChecked,
                bZipEngineTemplates = (bool)mainWindow.bIncludeTemplates.IsChecked,
                ZipEnginePath = mainWindow.ZipPath.Text
            };

            List<GitPlatform> GitPlatformList = mainWindow.context.SettingsJSON.GitDependencyPlatforms;
			IEnumerable<CheckBox> ComboBoxCollection = GetChildrenOfType<CheckBox>(mainWindow.PlatformStackPanelMain);
			foreach (GitPlatform gp in GitPlatformList)
			{
				string ComboBoxName = $"Git{gp.Name}Platform";
				foreach (CheckBox c in ComboBoxCollection)
                {
                    if (c.Name.ToLower() != ComboBoxName.ToLower()) 
                        continue;

                    gp.bIsIncluded = (bool)c.IsChecked;
                    break;
                }
			}
			BSJ.GitDependencyPlatforms = GitPlatformList;

			string JsonOutput = JsonConvert.SerializeObject(BSJ, Formatting.Indented);
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
				foreach (var gp in BSJ.GitDependencyPlatforms.Where(gp => gp.Name.ToLower() == InPlatform.ToLower()))
                {
                    gp.bIsIncluded = bIncluded;
                    break;
                }

				string JsonOutput = JsonConvert.SerializeObject(BSJ, Formatting.Indented);
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
			List<GitPlatform> GitPlatformList = mainWindow.context.SettingsJSON.GitDependencyPlatforms;
			IEnumerable<CheckBox> ComboBoxCollection = GetChildrenOfType<CheckBox>(mainWindow.PlatformStackPanelMain);

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

		public static IEnumerable<T> GetChildrenOfType<T>(DependencyObject dependencyObject) where T : DependencyObject
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
