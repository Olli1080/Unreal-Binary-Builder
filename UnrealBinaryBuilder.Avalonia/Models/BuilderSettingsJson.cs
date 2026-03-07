using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

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
        bIsIncluded = bInclude;
    }

    public string Name { get; set; }
    public bool bIsIncluded { get; set; }
}

public class BuilderSettingsJson
{
    // Application settings
    public string Theme { get; set; } = "Dark";
    public bool bCheckForUpdatesAtStartup { get; set; } = true;
    public bool bEnableDDCMessages { get; set; } = true;
    public bool bEnableEngineBuildConfirmationMessage { get; set; } = true;
    public bool bShowHTML5DeprecatedMessage { get; set; } = true;
    public bool bShowConsoleDeprecatedMessage { get; set; } = true;

    // Window settings
    public double WindowWidth { get; set; } = 1100;
    public double WindowHeight { get; set; } = 800;
    public double? WindowLeft { get; set; }
    public double? WindowTop { get; set; }
    public bool bWindowMaximized { get; set; }

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

    public bool bHostPlatformOnly { get; set; }
    public bool bHostPlatformEditorOnly { get; set; }
    public bool bWithWin64 { get; set; } = true;
    public bool bWithWin32 { get; set; } = true;
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

    public bool bWithDDC { get; set; } = true;
    public bool bHostPlatformDDCOnly { get; set; } = true;
    public bool bSignExecutables { get; set; }
    public bool bEnableSymStore { get; set; }
    public bool bWithFullDebugInfo { get; set; }
    public bool bCleanBuild { get; set; }
    public bool bWithWin64NoPCH { get; set; }
    public bool bWithServer { get; set; }
    public bool bWithClient { get; set; }
    public bool bCompileDatasmithPlugins { get; set; }
    public bool bShutdownPC { get; set; }
    public bool bShutdownIfBuildSuccess { get; set; }
    public bool bContinueToEngineBuild { get; set; } = true;
    public bool bBuildSetupBatFile { get; set; } = true;
    public bool bGenerateProjectFiles { get; set; } = true;
    public bool bBuildAutomationTool { get; set; } = true;

    public bool bZipEngineBuild { get; set; }
    public bool bZipEnginePDB { get; set; } = true;
    public bool bZipEngineDebug { get; set; }
    public bool bZipEngineDocumentation { get; set; } = true;
    public bool bZipEngineExtras { get; set; } = true;
    public bool bZipEngineSource { get; set; } = true;
    public bool bZipEngineFeaturePacks { get; set; } = true;
    public bool bZipEngineSamples { get; set; } = true;
    public bool bZipEngineTemplates { get; set; } = true;
    public bool bZipEngineFastCompression { get; set; } = true;
    public string ZipEnginePath { get; set; } = "";

    public VisualStudio? VisualStudio { get; set; }
}
