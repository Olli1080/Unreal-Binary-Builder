using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using UnrealBinaryBuilder.Avalonia.ViewModels;
using UnrealBinaryBuilder.Avalonia.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Classes;

namespace UnrealBinaryBuilder.Avalonia;

public partial class App : Application
{
    public IServiceProvider? Services { get; private set; }
    public static new App? Current => (App?)Application.Current;

    public override void Initialize()
    {
        // Force load AvaloniaEdit assembly before XAML loading
        var _ = typeof(AvaloniaEdit.TextEditor).Assembly;
        AvaloniaXamlLoader.Load(this);

        var services = new ServiceCollection();

        // Register Services
        services.AddSingleton<IProcessExecutor, ProcessExecutor>();
        services.AddSingleton<IUBBUpdater, UBBUpdater>();
        
        if (OperatingSystem.IsWindows()) services.AddSingleton<IPlatformService, WindowsPlatformService>();
        else if (OperatingSystem.IsLinux()) services.AddSingleton<IPlatformService, LinuxPlatformService>();
        else if (OperatingSystem.IsMacOS()) services.AddSingleton<IPlatformService, MacOSPlatformService>();

        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IUnrealEngineProvider, UnrealEngineProvider>();
        services.AddSingleton<IPluginsService, PluginsService>();

        // Register ViewModels
        services.AddTransient<MainWindowViewModel>();

        Services = services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services?.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
