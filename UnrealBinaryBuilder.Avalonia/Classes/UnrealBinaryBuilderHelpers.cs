using System.IO;
using System.Reflection;
using System;
using System.Text.RegularExpressions;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public static class PathHelpers
{
    public static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        return path.Replace('\\', '/').TrimEnd('/');
    }

    public static string ToUnixPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        return path.Replace('\\', '/');
    }

    public static string ToWindowsPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        return path.Replace('/', '\\');
    }

    public static string EnsureTrailingSlash(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "/";
        string normalized = ToUnixPath(path);
        if (!normalized.EndsWith('/')) return normalized + "/";
        return normalized;
    }

    public static string GetParentDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        string normalized = NormalizePath(path);
        int lastIndex = normalized.LastIndexOf('/');
        if (lastIndex == -1) return string.Empty;
        return normalized.Substring(0, lastIndex);
    }
}

public static class UnrealBinaryBuilderHelpers
{
    public static readonly string SetupBatFileName = "Setup.bat";
    public static readonly string GenerateProjectBatFileName = "GenerateProjectFiles.bat";
    public static readonly string AUTOMATION_TOOL_NAME = "AutomationTool";
    public static readonly string AUTOMATION_TOOL_LAUNCHER_NAME = $"{AUTOMATION_TOOL_NAME}Launcher";
    public static readonly string DEFAULT_BUILD_XML_FILE = "Engine/Build/InstalledEngineBuild.xml";

    public static string GetProductVersionString()
    {
        Version? version = Assembly.GetEntryAssembly()?.GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
    }

    public static string? GetEngineVersion(string enginePath)
    {
        if (string.IsNullOrWhiteSpace(enginePath)) return null;
        string versionFile = Path.Combine(enginePath, "Engine", "Build", "Build.version");
        if (!File.Exists(versionFile)) return null;

        try
        {
            string content = File.ReadAllText(versionFile);
            string major = "0", minor = "0", patch = "0";
            var majorMatch = Regex.Match(content, @"""MajorVersion"":\s*(\d+)");
            var minorMatch = Regex.Match(content, @"""MinorVersion"":\s*(\d+)");
            var patchMatch = Regex.Match(content, @"""PatchVersion"":\s*(\d+)");

            if (majorMatch.Success) major = majorMatch.Groups[1].Value;
            if (minorMatch.Success) minor = minorMatch.Groups[1].Value;
            if (patchMatch.Success) patch = patchMatch.Groups[1].Value;

            return $"{major}.{minor}.{patch}";
        }
        catch { return null; }
    }

    public static string ProgrammsPath(string BaseEnginePath) => Path.Combine(BaseEnginePath, "Engine", "Source", "Programs");

    public static string? GetAutomationToolProjectFile(string BaseEnginePath)
    {
        if (string.IsNullOrWhiteSpace(BaseEnginePath)) return null;
        return Path.Combine(ProgrammsPath(BaseEnginePath), AUTOMATION_TOOL_NAME, $"{AUTOMATION_TOOL_NAME}.csproj");
    }

    public static string? GetAutomationToolLauncherProjectFile(string BaseEnginePath)
    {
        if (string.IsNullOrWhiteSpace(BaseEnginePath)) return null;
        return Path.Combine(ProgrammsPath(BaseEnginePath), AUTOMATION_TOOL_LAUNCHER_NAME, $"{AUTOMATION_TOOL_LAUNCHER_NAME}.csproj");
    }

    public static string AutomationPath(string BaseEnginePath, bool isUE5)
    {
        string dotnet = Path.Combine(BaseEnginePath, "Engine", "Binaries", "DotNET");
        if (isUE5) return Path.Combine(dotnet, AUTOMATION_TOOL_NAME, $"{AUTOMATION_TOOL_NAME}.exe");
        return Path.Combine(dotnet, $"{AUTOMATION_TOOL_LAUNCHER_NAME}.exe");
    }
}
