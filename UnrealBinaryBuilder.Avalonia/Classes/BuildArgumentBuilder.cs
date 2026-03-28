using UnrealBinaryBuilder.Avalonia.Models;
using UnrealBinaryBuilder.Avalonia.ViewModels;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public static class BuildArgumentBuilder
{
    public static BuildGraphArguments BuildEngineArguments(BuilderSettingsJson settings, UnrealEngineMetadata? metadata, VisualStudioVersion? selectedVsVersion)
    {
        var args = new BuildGraphArguments
        {
            Target = "Make Installed Build Win64",
            Script = settings.CustomBuildFile ?? UnrealBinaryBuilderHelpers.DEFAULT_BUILD_XML_FILE,
            Clean = settings.CleanBuild,
            CustomOptions = settings.CustomOptions ?? ""
        };

        string configs = string.Join(";", settings.GameConfigurations);
        
        args.SetFlags["WithDDC"] = GetBoolStr(settings.WithDDC);
        args.SetFlags["SignExecutables"] = GetBoolStr(settings.SignExecutables);
        args.SetFlags["EmbedSrcSrvInfo"] = GetBoolStr(settings.EnableSymStore);
        args.SetFlags["GameConfigurations"] = configs;
        args.SetFlags["WithFullDebugInfo"] = GetBoolStr(settings.WithFullDebugInfo);
        args.SetFlags["HostPlatformOnly"] = GetBoolStr(settings.HostPlatformOnly);
        args.SetFlags["HostPlatformEditorOnly"] = GetBoolStr(settings.HostPlatformEditorOnly);
        args.SetFlags["AllowParallelExecutor"] = GetBoolStr(settings.AllowParallelExecutor);
        args.SetFlags["SignWindowsExecutablesInParallel"] = GetBoolStr(settings.SignWindowsExecutablesInParallel);
        args.SetFlags["IncludeDocs"] = GetBoolStr(settings.IncludeDocs);
        args.SetFlags["AllPlatforms"] = GetBoolStr(settings.AllPlatforms);

        if (!string.IsNullOrEmpty(settings.BuildIdOverride))
        {
            args.SetFlags["BuildIdOverride"] = settings.BuildIdOverride;
        }

        if (!string.IsNullOrEmpty(settings.ExtraCompileArgs))
        {
            args.SetFlags["ExtraCompileArgs"] = settings.ExtraCompileArgs;
        }

        if (!string.IsNullOrEmpty(settings.ExtraCompileArgsMac))
        {
            args.SetFlags["ExtraCompileArgsMac"] = settings.ExtraCompileArgsMac;
        }

        if (!string.IsNullOrEmpty(settings.ExtraDDCArgs))
        {
            args.SetFlags["ExtraDDCArgs"] = settings.ExtraDDCArgs;
        }

        if (settings.WithDDC && settings.HostPlatformDDCOnly)
        {
            args.SetFlags["HostPlatformDDCOnly"] = "true";
        }
        
        if (settings.HostPlatformOnly)
        {
            args.SetFlags["HostPlatformOnly"] = "true";
        }
        else
        {
            if (metadata?.SupportWin32 == true)
            {
                args.SetFlags["WithWin32"] = GetBoolStr(settings.WithWin32);
            }
            
            args.SetFlags["WithWin64"] = GetBoolStr(settings.WithWin64);
            args.SetFlags["WithMac"] = GetBoolStr(settings.WithMac);
            args.SetFlags["WithAndroid"] = GetBoolStr(settings.WithAndroid);
            args.SetFlags["WithIOS"] = GetBoolStr(settings.WithIOS);
            args.SetFlags["WithTVOS"] = GetBoolStr(settings.WithTVOS);
            args.SetFlags["WithLinux"] = GetBoolStr(settings.WithLinux);
            args.SetFlags["WithLumin"] = GetBoolStr(settings.WithLumin);

            if (metadata?.SupportWinArm64 == true)
            {
                args.SetFlags["WithWinArm64"] = GetBoolStr(settings.WithWinArm64);
                args.SetFlags["WithWinArm64ec"] = GetBoolStr(settings.WithWinArm64ec);
            }

            if (metadata?.SupportVisionOS == true)
            {
                args.SetFlags["WithVisionOS"] = GetBoolStr(settings.WithVisionOS);
            }

            if (metadata?.SupportHTML5 == true)
            {
                args.SetFlags["WithHTML5"] = GetBoolStr(settings.WithHTML5);
            }

            if (metadata?.SupportConsoles == true)
            {
                args.SetFlags["WithSwitch"] = GetBoolStr(settings.WithSwitch);
                args.SetFlags["WithPS4"] = GetBoolStr(settings.WithPS4);
                args.SetFlags["WithXboxOne"] = GetBoolStr(settings.WithXboxOne);
            }

            if (metadata?.SupportLinuxArm64 == true)
            {
                args.SetFlags["WithLinuxArm64"] = GetBoolStr(settings.WithLinuxAArch64);
            }
            else if (metadata?.SupportLinuxAArch64 == true)
            {
                args.SetFlags["WithLinuxAArch64"] = GetBoolStr(settings.WithLinuxAArch64);
            }
        }

        if (!string.IsNullOrEmpty(settings.AnalyticsOverride))
        {
            args.SetFlags["AnalyticsTypeOverride"] = settings.AnalyticsOverride;
        }
        
        if (metadata?.SupportServerClientTargets == true)
        {
            args.SetFlags["WithServer"] = GetBoolStr(settings.WithServer);
            args.SetFlags["WithClient"] = GetBoolStr(settings.WithClient);
            args.SetFlags["WithHoloLens"] = GetBoolStr(settings.WithHoloLens);
        }

        if (metadata?.IsEngineSelection425OrAbove == true)
        {
            args.SetFlags["CompileDatasmithPlugins"] = GetBoolStr(settings.CompileDatasmithPlugins);
        }
        
        if (settings.WithWin64NoPCH)
        {
            args.SetFlags["WithWin64"] = "true";
            args.SetFlags["BuildWithPrecompiledHeader"] = "false";
        }

        if (selectedVsVersion != null)
        {
            args.SetFlags[$"VS{selectedVsVersion.Version}"] = "true";
        }

        return args;
    }

    public static UatArguments BuildPluginArguments(PluginCardViewModel plugin)
    {
        var args = new UatArguments
        {
            Command = "BuildPlugin"
        };

        args.Arguments["Plugin"] = plugin.PluginPath;
        args.Arguments["Package"] = plugin.DestinationPath;
        
        args.Flags.Add("Rocket");

        string platforms = string.Join("+", plugin.TargetPlatforms ?? ["Win64"]);
        args.Arguments["TargetPlatforms"] = platforms;

        return args;
    }

    private static string GetBoolStr(bool b) => b.ToString().ToLower();
}
