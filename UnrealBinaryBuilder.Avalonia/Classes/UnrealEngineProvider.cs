using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class UnrealEngineProvider : IUnrealEngineProvider
{
    public UnrealEngineMetadata? GetEngineMetadata(string enginePath)
    {
        if (string.IsNullOrEmpty(enginePath) || !Directory.Exists(enginePath)) return null;

        string versionFile = Path.Combine(enginePath, "Engine", "Build", "Build.version");
        if (!File.Exists(versionFile)) return null;

        try {
            string json = File.ReadAllText(versionFile);
            JObject o = JObject.Parse(json);
            int major = o.Value<int>("MajorVersion");
            int minor = o.Value<int>("MinorVersion");
            int patch = o.Value<int>("PatchVersion");
            string versionStr = $"{major}.{minor}";
            string fullVersionStr = $"{major}.{minor}.{patch}";

            bool isUE5 = major >= 5;
            bool isUE4 = major == 4;

            return new UnrealEngineMetadata(
                Major: major,
                Minor: minor,
                Patch: patch,
                VersionString: versionStr,
                FullVersionString: fullVersionStr,
                SupportWin32: isUE4 && minor < 23,
                SupportHTML5: isUE4 && minor < 24,
                SupportConsoles: isUE4 && minor < 25,
                IsEngineSelection425OrAbove: major >= 5 || minor >= 25,
                SupportServerClientTargets: major >= 5 || minor >= 21,
                SupportLinuxAArch64: isUE4 && minor >= 24,
                SupportLinuxArm64: isUE5,
                IsUE5: isUE5
            );
        } catch { return null; }
    }

    public string GetAutomationPath(string enginePath, bool isUE5)
    {
        string dotnet = Path.Combine(enginePath, "Engine", "Binaries", "DotNET");
        string exeName = isUE5 ? Path.Combine("AutomationTool", "AutomationTool.exe") : "AutomationToolLauncher.exe";
        return PathHelpers.ToUnixPath(Path.Combine(dotnet, exeName));
    }
}
