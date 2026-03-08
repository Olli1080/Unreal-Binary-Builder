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
}
