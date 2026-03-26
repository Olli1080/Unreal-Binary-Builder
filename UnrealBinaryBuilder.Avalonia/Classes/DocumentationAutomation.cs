using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using FluentAvalonia.UI.Controls;
using UnrealBinaryBuilder.Avalonia.Views;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public static class DocumentationAutomation
{
    public static async Task GenerateScreenshots(MainWindow window)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string docDir;

        // Check if we are running from the source repository (git clone)
        // Typical path: root/UnrealBinaryBuilder.Avalonia/bin/Debug/net10.0/
        string repoRootCandidate = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        if (File.Exists(Path.Combine(repoRootCandidate, "UnrealBinaryBuilder.sln")))
        {
            docDir = Path.Combine(repoRootCandidate, "Documentation");
        }
        else
        {
            // Fallback for built/distributed version: Save to a local folder
            docDir = Path.Combine(baseDir, "Documentation");
        }

        if (!Directory.Exists(docDir)) Directory.CreateDirectory(docDir);

        var nav = window.FindControl<NavigationView>("MainNavigation");
        var tabs = window.FindControl<TabControl>("EngineTabControl");

        if (nav == null || tabs == null) return;

        // Screenshot 1: Engine/Setup section
        await SetNavigation(nav, "Engine");
        tabs.SelectedIndex = 0;
        await Task.Delay(500); // Wait for layout/animations
        Capture(window, Path.Combine(docDir, "screenshot_1.png"));

        // Screenshot 2: Engine/Compile section
        tabs.SelectedIndex = 2;
        await Task.Delay(500);
        Capture(window, Path.Combine(docDir, "screenshot_2.png"));

        // Screenshot 3: Engine/Zip Build section
        tabs.SelectedIndex = 1;
        await Task.Delay(500);
        Capture(window, Path.Combine(docDir, "screenshot_3.png"));

        // Screenshot 4: Plugins Section
        await SetNavigation(nav, "Plugins");
        await Task.Delay(500);
        Capture(window, Path.Combine(docDir, "screenshot_4.png"));

        Console.WriteLine($"Screenshots generated in: {docDir}");
    }

    private static async Task SetNavigation(NavigationView nav, string tag)
    {
        foreach (var item in nav.MenuItems)
        {
            if (item is NavigationViewItem nvi && nvi.Tag?.ToString() == tag)
            {
                nav.SelectedItem = nvi;
                break;
            }
        }
        await Task.Delay(100);
    }

    private static void Capture(Control control, string filePath)
    {
        var pixelSize = new PixelSize((int)control.Bounds.Width, (int)control.Bounds.Height);
        var bitmap = new RenderTargetBitmap(pixelSize, new Vector(96, 96));
        bitmap.Render(control);
        bitmap.Save(filePath);
    }
}
