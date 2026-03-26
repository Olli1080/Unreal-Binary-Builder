using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using UnrealBinaryBuilder.Avalonia.ViewModels;
using UnrealBinaryBuilder.Avalonia.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Classes.Logging;

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

        // Register Infrastructure
        services.AddSingleton<ITelemetryService, GameAnalyticsTelemetryService>();
        services.AddSingleton<IProcessExecutor, ProcessExecutor>();

        // Register Log Sinks
        services.AddSingleton<UiLogSink>();
        services.AddSingleton<ILogSink>(sp => sp.GetRequiredService<UiLogSink>());
        services.AddSingleton<ILogSink, FileLogSink>();
        services.AddSingleton<ILogSink>(sp => new TelemetryLogSink(sp.GetRequiredService<ITelemetryService>()));
        services.AddSingleton<IUBBLogger, AggregateLogger>();

        services.AddSingleton<IVelopackUpdaterService, VelopackUpdaterService>();
        
        if (OperatingSystem.IsWindows()) services.AddSingleton<IPlatformService, WindowsPlatformService>();
        else if (OperatingSystem.IsLinux()) services.AddSingleton<IPlatformService, LinuxPlatformService>();
        else if (OperatingSystem.IsMacOS()) services.AddSingleton<IPlatformService, MacOSPlatformService>();

        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IUnrealEngineProvider, UnrealEngineProvider>();
        services.AddSingleton<IPluginsService, PluginsService>();
        services.AddSingleton<IUIService>(sp => new UIService(sp.GetRequiredService<IPlatformService>(), sp.GetRequiredService<ITelemetryService>()));
        services.AddSingleton<ISetupService>(sp => new SetupService(sp.GetRequiredService<IProcessExecutor>(), sp.GetRequiredService<IUBBLogger>(), sp.GetRequiredService<ITelemetryService>()));
        services.AddSingleton<IZipService, ZipService>();
        services.AddSingleton<IEngineBuildService, EngineBuildService>();
        services.AddSingleton<IPluginBuildService, PluginBuildService>();
        services.AddSingleton<IGitService, GitService>();
        services.AddSingleton<IBuildTimerService, BuildTimerService>();
        services.AddSingleton<IBuildHistoryService, BuildHistoryService>();
        services.AddSingleton<IBuildOrchestrationService, BuildOrchestrationService>();
        services.AddSingleton<ILogFormatterService, LogFormatterService>();

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
