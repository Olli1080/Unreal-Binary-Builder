using System.Collections.Generic;
using System.Linq;
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
            Clean = settings.bCleanBuild,
            CustomOptions = settings.CustomOptions ?? ""
        };

        string configs = string.Join(";", settings.GameConfigurations);
        
        args.SetFlags["WithDDC"] = GetBoolStr(settings.bWithDDC);
        args.SetFlags["SignExecutables"] = GetBoolStr(settings.bSignExecutables);
        args.SetFlags["EmbedSrcSrvInfo"] = GetBoolStr(settings.bEnableSymStore);
        args.SetFlags["GameConfigurations"] = configs;
        args.SetFlags["WithFullDebugInfo"] = GetBoolStr(settings.bWithFullDebugInfo);
        args.SetFlags["HostPlatformOnly"] = GetBoolStr(settings.bHostPlatformOnly);
        args.SetFlags["HostPlatformEditorOnly"] = GetBoolStr(settings.bHostPlatformEditorOnly);

        if (settings.bWithDDC && settings.bHostPlatformDDCOnly)
        {
            args.SetFlags["HostPlatformDDCOnly"] = "true";
        }
        
        if (settings.bHostPlatformOnly)
        {
            args.SetFlags["HostPlatformOnly"] = "true";
        }
        else
        {
            if (metadata?.SupportWin32 == true)
            {
                args.SetFlags["WithWin32"] = GetBoolStr(settings.bWithWin32);
            }
            
            args.SetFlags["WithWin64"] = GetBoolStr(settings.bWithWin64);
            args.SetFlags["WithMac"] = GetBoolStr(settings.bWithMac);
            args.SetFlags["WithAndroid"] = GetBoolStr(settings.bWithAndroid);
            args.SetFlags["WithIOS"] = GetBoolStr(settings.bWithIOS);
            args.SetFlags["WithTVOS"] = GetBoolStr(settings.bWithTVOS);
            args.SetFlags["WithLinux"] = GetBoolStr(settings.bWithLinux);
            args.SetFlags["WithLumin"] = GetBoolStr(settings.bWithLumin);

            if (metadata?.SupportHTML5 == true)
            {
                args.SetFlags["WithHTML5"] = GetBoolStr(settings.bWithHTML5);
            }

            if (metadata?.SupportConsoles == true)
            {
                args.SetFlags["WithSwitch"] = GetBoolStr(settings.bWithSwitch);
                args.SetFlags["WithPS4"] = GetBoolStr(settings.bWithPS4);
                args.SetFlags["WithXboxOne"] = GetBoolStr(settings.bWithXboxOne);
            }

            if (metadata?.SupportLinuxArm64 == true)
            {
                args.SetFlags["WithLinuxArm64"] = GetBoolStr(settings.bWithLinuxAArch64);
            }
            else if (metadata?.SupportLinuxAArch64 == true)
            {
                args.SetFlags["WithLinuxAArch64"] = GetBoolStr(settings.bWithLinuxAArch64);
            }
        }

        if (!string.IsNullOrEmpty(settings.AnalyticsOverride))
        {
            args.SetFlags["AnalyticsTypeOverride"] = settings.AnalyticsOverride;
        }
        
        if (metadata?.SupportServerClientTargets == true)
        {
            args.SetFlags["WithServer"] = GetBoolStr(settings.bWithServer);
            args.SetFlags["WithClient"] = GetBoolStr(settings.bWithClient);
            args.SetFlags["WithHoloLens"] = GetBoolStr(settings.bWithHoloLens);
        }

        if (metadata?.IsEngineSelection425OrAbove == true)
        {
            args.SetFlags["CompileDatasmithPlugins"] = GetBoolStr(settings.bCompileDatasmithPlugins);
        }
        
        if (settings.bWithWin64NoPCH)
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
