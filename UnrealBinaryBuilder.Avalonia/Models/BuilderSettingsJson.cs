using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace UnrealBinaryBuilder.Avalonia.Models;

public enum BuildConfiguration
{
    Debug,
    DebugGame,
    Development,
    Shipping,
    Test
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
        IsIncluded = bInclude;
    }

    public string Name { get; set; }

    [JsonProperty("bIsIncluded")]
    public bool IsIncluded { get; set; }
}

public class BuilderSettingsJson
{
    // Application settings
    public string Theme { get; set; } = "Dark";

    [JsonProperty("bCheckForUpdatesAtStartup")]
    public bool CheckForUpdatesAtStartup { get; set; } = true;

    [JsonProperty("bEnableDDCMessages")]
    public bool EnableDDCMessages { get; set; } = true;

    [JsonProperty("bEnableEngineBuildConfirmationMessage")]
    public bool EnableEngineBuildConfirmationMessage { get; set; } = true;

    [JsonProperty("bShowHTML5DeprecatedMessage")]
    public bool ShowHTML5DeprecatedMessage { get; set; } = true;

    [JsonProperty("bShowConsoleDeprecatedMessage")]
    public bool ShowConsoleDeprecatedMessage { get; set; } = true;

    // Window settings
    public double WindowWidth { get; set; } = 1100;
    public double WindowHeight { get; set; } = 800;
    public double? WindowLeft { get; set; }
    public double? WindowTop { get; set; }

    [JsonProperty("bWindowMaximized")]
    public bool WindowMaximized { get; set; }

    public string? SetupBatFile { get; set; }
    public string? CustomBuildFile { get; set; }
    public HashSet<BuildConfiguration> GameConfigurations { get; set; } = [BuildConfiguration.Development, BuildConfiguration.Shipping];
    public string? CustomOptions { get; set; }
    public string? AnalyticsOverride { get; set; }

    public bool GitDependencyAll { get; set; } = true;
    public List<GitPlatform> GitDependencyPlatforms { get; set; } = new();
    public int GitDependencyThreads { get; set; } = 4;
    public int GitDependencyMaxRetries { get; set; } = 4;
    public string GitDependencyProxy { get; set; } = "";
    public bool GitDependencyEnableCache { get; set; } = true;
    public string GitDependencyCache { get; set; } = "";
    public double GitDependencyCacheMultiplier { get; set; } = 2.0;
    public int GitDependencyCacheDays { get; set; } = 7;

    [JsonProperty("bHostPlatformOnly")]
    public bool HostPlatformOnly { get; set; }

    [JsonProperty("bHostPlatformEditorOnly")]
    public bool HostPlatformEditorOnly { get; set; }

    [JsonProperty("bWithWin64")]
    public bool WithWin64 { get; set; } = true;

    [JsonProperty("bWithWin32")]
    public bool WithWin32 { get; set; } = true;

    [JsonProperty("bWithMac")]
    public bool WithMac { get; set; }

    [JsonProperty("bWithLinux")]
    public bool WithLinux { get; set; }

    [JsonProperty("bWithLinuxAArch64")]
    public bool WithLinuxAArch64 { get; set; }

    [JsonProperty("bWithAndroid")]
    public bool WithAndroid { get; set; }

    [JsonProperty("bWithIOS")]
    public bool WithIOS { get; set; }

    [JsonProperty("bWithHTML5")]
    public bool WithHTML5 { get; set; }

    [JsonProperty("bWithTVOS")]
    public bool WithTVOS { get; set; }

    [JsonProperty("bWithSwitch")]
    public bool WithSwitch { get; set; }

    [JsonProperty("bWithPS4")]
    public bool WithPS4 { get; set; }

    [JsonProperty("bWithXboxOne")]
    public bool WithXboxOne { get; set; }

    [JsonProperty("bWithLumin")]
    public bool WithLumin { get; set; }

    [JsonProperty("bWithHoloLens")]
    public bool WithHoloLens { get; set; }

    [JsonProperty("bWithDDC")]
    public bool WithDDC { get; set; } = true;

    [JsonProperty("bHostPlatformDDCOnly")]
    public bool HostPlatformDDCOnly { get; set; } = true;

    [JsonProperty("bSignExecutables")]
    public bool SignExecutables { get; set; }

    [JsonProperty("bEnableSymStore")]
    public bool EnableSymStore { get; set; }

    [JsonProperty("bWithFullDebugInfo")]
    public bool WithFullDebugInfo { get; set; }

    [JsonProperty("bCleanBuild")]
    public bool CleanBuild { get; set; }

    [JsonProperty("bWithWin64NoPCH")]
    public bool WithWin64NoPCH { get; set; }

    [JsonProperty("bWithServer")]
    public bool WithServer { get; set; }

    [JsonProperty("bWithClient")]
    public bool WithClient { get; set; }

    [JsonProperty("bCompileDatasmithPlugins")]
    public bool CompileDatasmithPlugins { get; set; }

    [JsonProperty("bShutdownPC")]
    public bool ShutdownPC { get; set; }

    [JsonProperty("bShutdownIfBuildSuccess")]
    public bool ShutdownIfBuildSuccess { get; set; }

    [JsonProperty("bContinueToEngineBuild")]
    public bool ContinueToEngineBuild { get; set; } = true;

    [JsonProperty("bBuildSetupBatFile")]
    public bool BuildSetupBatFile { get; set; } = true;

    [JsonProperty("bGenerateProjectFiles")]
    public bool GenerateProjectFiles { get; set; } = true;

    [JsonProperty("bBuildAutomationTool")]
    public bool BuildAutomationTool { get; set; } = true;

    [JsonProperty("bZipEngineBuild")]
    public bool ZipEngineBuild { get; set; }

    [JsonProperty("bZipEnginePDB")]
    public bool ZipEnginePDB { get; set; } = true;

    [JsonProperty("bZipEngineDebug")]
    public bool ZipEngineDebug { get; set; }

    [JsonProperty("bZipEngineDocumentation")]
    public bool ZipEngineDocumentation { get; set; } = true;

    [JsonProperty("bZipEngineExtras")]
    public bool ZipEngineExtras { get; set; } = true;

    [JsonProperty("bZipEngineSource")]
    public bool ZipEngineSource { get; set; } = true;

    [JsonProperty("bZipEngineFeaturePacks")]
    public bool ZipEngineFeaturePacks { get; set; } = true;

    [JsonProperty("bZipEngineSamples")]
    public bool ZipEngineSamples { get; set; } = true;

    [JsonProperty("bZipEngineTemplates")]
    public bool ZipEngineTemplates { get; set; } = true;

    [JsonProperty("bZipEngineFastCompression")]
    public bool ZipEngineFastCompression { get; set; } = true;

    public string ZipEnginePath { get; set; } = "";

    public VisualStudio? VisualStudio { get; set; }
}
