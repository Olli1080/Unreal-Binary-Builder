using System;
using System.Diagnostics;
using System.IO;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class WindowsPlatformService : IPlatformService
{
    public void OpenFolder(string path)
    {
        if (Directory.Exists(path))
        {
            Process.Start("explorer.exe", path);
        }
    }

    public void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    public void ShutdownPC(int seconds)
    {
        Process.Start("shutdown", $"/s /t {seconds}");
    }
}

public class LinuxPlatformService : IPlatformService
{
    public void OpenFolder(string path)
    {
        if (Directory.Exists(path))
        {
            Process.Start("xdg-open", path);
        }
    }

    public void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    public void ShutdownPC(int seconds)
    {
        Process.Start("shutdown", $"-h +{Math.Max(1, seconds / 60)}");
    }
}

public class MacOSPlatformService : IPlatformService
{
    public void OpenFolder(string path)
    {
        if (Directory.Exists(path))
        {
            Process.Start("open", path);
        }
    }

    public void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    public void ShutdownPC(int seconds)
    {
        Process.Start("shutdown", $"-h +{Math.Max(1, seconds / 60)}");
    }
}
