using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class VisualStudioMsBuild
{
    public static VisualStudioMsBuild? ParseMsBuild(string path)
    {
        VisualStudioMsBuild msBuild = new VisualStudioMsBuild
        {
            _edition = Path.GetFileName(path)
        };

        string msBuildPath = Path.Combine(path, "MSBuild");
        if (Directory.Exists(Path.Combine(msBuildPath, "Current")))
            msBuildPath = Path.Combine(msBuildPath, "Current");

        if (Directory.Exists(msBuildPath))
        {
            foreach (string exePath in Directory.GetFiles(msBuildPath, "msbuild.exe", SearchOption.AllDirectories))
            {
                string? architecture = Path.GetFileName(Path.GetDirectoryName(exePath));
                if (architecture == "amd64")
                    msBuild._x64 = exePath;
                else if (architecture == "Bin")
                    msBuild._x32 = exePath;
            }
        }

        if (!string.IsNullOrEmpty(msBuild._x64) || !string.IsNullOrEmpty(msBuild._x32))
            return msBuild;

        return null;
    }

    public string Edition => _edition;
    public string X64Path => _x64;
    public string X32Path => _x32;

    private string _edition = string.Empty;
    private string _x64 = string.Empty;
    private string _x32 = string.Empty;
}

public class VisualStudioVersion
{
    public static VisualStudioVersion? ParseVersion(string path)
    {
        if (!int.TryParse(Path.GetFileName(path), out int parsedVersion))
        {
            return null;
        }

        VisualStudioVersion version = new VisualStudioVersion
        {
            _version = parsedVersion
        };

        foreach (var directory in Directory.GetDirectories(path))
        {
            var build = VisualStudioMsBuild.ParseMsBuild(directory);
            if (build != null)
                version._msBuilds.Add(build);
        }

        return version._msBuilds.Count != 0 ? version : null;
    }

    public int Version => _version;
    public List<VisualStudioMsBuild> MsBuilds => _msBuilds;

    private int _version;
    private List<VisualStudioMsBuild> _msBuilds = [];
}

public class VisualStudioConfigurations
{
    public const string MSVC = "Microsoft Visual Studio";

    public static string? GetX86()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Environment.GetEnvironmentVariable("ProgramFiles(x86)") : null;
    }

    public static string? GetX64()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Environment.GetEnvironmentVariable("ProgramW6432") : null;
    }

    public VisualStudioConfigurations()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        string? x86Path = GetX86();
        string? x64Path = GetX64();

        if (x86Path != null)
            x86Path = Path.Combine(x86Path, MSVC);

        if (x64Path != null)
            x64Path = Path.Combine(x64Path, MSVC);

        List<string> visualStudioPaths = [];

        if (x86Path != null && Directory.Exists(x86Path))
            visualStudioPaths.Add(x86Path);

        if (x64Path != null && Directory.Exists(x64Path))
            visualStudioPaths.Add(x64Path);

        foreach (var topLevelDir in visualStudioPaths)
        {
            IEnumerable<string> potentialVersions = from dir in Directory.GetDirectories(topLevelDir)
                                                    where int.TryParse(Path.GetFileName(dir), out _)
                                                    select dir;

            foreach (var potentialVersion in potentialVersions)
            {
                VisualStudioVersion? version = VisualStudioVersion.ParseVersion(potentialVersion);
                if (version != null)
                    _versions.Add(version);
            }
        }
    }

    public List<VisualStudioVersion> Versions => _versions;

    private List<VisualStudioVersion> _versions = [];
}
