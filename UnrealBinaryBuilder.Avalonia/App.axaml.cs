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

        SetupGlobalExceptionHandling();

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
        services.AddSingleton<IAppInitializer>(sp => new AppInitializer(
            sp.GetRequiredService<ITelemetryService>(),
            sp.GetRequiredService<ISettingsService>(),
            sp.GetRequiredService<IVelopackUpdaterService>(),
            sp.GetRequiredService<IUIService>(),
            sp.GetRequiredService<IUBBLogger>()));
        
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
        services.AddSingleton<IBuildPipeline, BuildPipeline>();
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
            // Run startup sequence
            _ = Services?.GetRequiredService<IAppInitializer>().InitializeAsync();

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

    private void SetupGlobalExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
            e.SetObserved();
        };
    }

    private void LogUnhandledException(Exception ex, string source)
    {
        // Try to log it via our services if they are ready
        try
        {
            var logger = Services?.GetService<IUBBLogger>();
            logger?.Error(ex, $"Unhandled Exception from {source}", LogCategory.General);

            var telemetry = Services?.GetService<ITelemetryService>();
            telemetry?.TrackError($"Unhandled Exception: {ex.Message} (Source: {source})", TelemetrySeverity.Critical);

            // Show a message to the user if possible
            Dispatcher.UIThread.Post(async () =>
            {
                var uiService = Services?.GetService<IUIService>();
                if (uiService != null)
                {
                    await uiService.ShowMessageDialog("Unexpected Error", 
                        $"An unexpected error occurred: {ex.Message}\n\nCheck the log for more details.", 
                        "OK");
                }
            });
        }
        catch
        {
            // Fallback if services are not ready or fail
            Console.WriteLine($"FATAL: {ex}");
        }
    }
}
