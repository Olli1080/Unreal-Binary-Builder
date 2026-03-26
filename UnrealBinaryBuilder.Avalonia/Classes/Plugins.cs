using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public interface IPluginsService
{
    List<EngineBuild>? GetInstalledEpicEngines();
    List<EngineBuild>? GetInstalledCustomEngines();
    List<EngineBuild> GetInstalledEngines();
}

public class PluginsService : IPluginsService
{
    private readonly IUnrealEngineProvider _ueProvider;

    public PluginsService(IUnrealEngineProvider ueProvider)
    {
        _ueProvider = ueProvider;
    }

    public List<EngineBuild>? GetInstalledEpicEngines()
    {
        List<EngineBuild> returnValue = [];
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var engineInstallations = Registry.LocalMachine.OpenSubKey(@"Software\EpicGames\Unreal Engine");
            if (engineInstallations != null)
            {
                string[] installedEngines = engineInstallations.GetSubKeyNames();
                foreach (var s in installedEngines)
                {
                    using var installedDirectoryKey = engineInstallations.OpenSubKey(s);
                    object? o = installedDirectoryKey?.GetValue("InstalledDirectory");

                    if (o is not string enginePath) continue;
                    if (!Directory.Exists(enginePath)) continue;

                    EngineBuild engineBuild = new EngineBuild
                    {
                        IsCustomEngine = false,
                        EngineAssociation = s,
                        EngineName = s,
                        EnginePath = enginePath
                    };

                    returnValue.Add(engineBuild);
                }
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string sharedPath = "/Users/Shared/Epic Games";
            if (Directory.Exists(sharedPath))
            {
                foreach (var dir in Directory.GetDirectories(sharedPath))
                {
                    string folderName = Path.GetFileName(dir);
                    if (folderName.StartsWith("UE_"))
                    {
                        returnValue.Add(new EngineBuild
                        {
                            IsCustomEngine = false,
                            EngineAssociation = folderName,
                            EngineName = folderName,
                            EnginePath = dir
                        });
                    }
                }
            }
        }
        
        return returnValue.Count > 0 ? returnValue : null;
    }

    public List<EngineBuild>? GetInstalledCustomEngines()
    {
        List<EngineBuild> returnValue = [];
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var customEngineInstallations = Registry.CurrentUser.OpenSubKey(@"Software\Epic Games\Unreal Engine\Builds");

            if (customEngineInstallations != null)
            {
                string[] installedEngines = customEngineInstallations.GetValueNames();
                foreach (var s in installedEngines)
                {
                    object? o = customEngineInstallations.GetValue(s);

                    if (o is not string baseEnginePath) continue;
                    if (!Directory.Exists(baseEnginePath)) continue;

                    var metadata = _ueProvider.GetEngineMetadata(baseEnginePath);
                    if (metadata == null) continue;

                    EngineBuild engineBuild = new EngineBuild
                    {
                        IsCustomEngine = true,
                        EngineAssociation = s,
                        EngineName = $"{metadata.FullVersionString} (Custom) {s}",
                        EnginePath = baseEnginePath
                    };
                    returnValue.Add(engineBuild);
                }
            }
        }
        
        return returnValue.Count > 0 ? returnValue : null;
    }

    public List<EngineBuild> GetInstalledEngines()
    {
        return [.. GetInstalledEpicEngines() ?? [], .. GetInstalledCustomEngines() ?? []];
    }
}

public class UE4PluginJson
{
    public string FriendlyName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MarketplaceURL { get; set; } = string.Empty;
    public bool IsBetaVersion { get; set; }
    public bool IsExperimentalVersion { get; set; }
    public IList<UE4PluginModule> Modules { get; set; } = [];
}

public class UE4PluginModule
{
    public IList<string> WhitelistPlatforms { get; set; } = [];
}

public class EngineBuild
{
    public string EngineName { get; set; } = string.Empty;
    public string EnginePath { get; set; } = string.Empty;
    public bool IsCustomEngine { get; set; }
    public string EngineAssociation { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        if (obj is EngineBuild build)
        {
            return build.EngineName == EngineName;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return EngineName.GetHashCode();
    }
}
