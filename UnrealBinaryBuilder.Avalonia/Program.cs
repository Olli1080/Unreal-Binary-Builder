using Avalonia;
using System;
using Sentry;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Views.UserControls;
using Avalonia.Controls.ApplicationLifetimes;
using UnrealBinaryBuilder.Avalonia.Classes;

namespace UnrealBinaryBuilder.Avalonia;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        /* Sentry is disabled for this fork until a new DSN is provided.
        SentrySdk.Init(options =>
        {
            options.Dsn = "https://23f478ac8a004c5782a7f6597c0b0325@o502371.ingest.sentry.io/5584682";
            options.SendDefaultPii = true;
            options.MaxBreadcrumbs = 50;
            options.AttachStacktrace = true;
            options.AutoSessionTracking = true;
            options.Release = UnrealBinaryBuilderHelpers.GetProductVersionString();
            #if DEBUG
            options.Debug = true;
            #endif
        });
        */

        AppDomain.CurrentDomain.UnhandledException += (s, e) => 
            ShowCrashReporter(e.ExceptionObject as Exception);
        
        TaskScheduler.UnobservedTaskException += (s, e) => 
        {
            ShowCrashReporter(e.Exception);
            e.SetObserved();
        };

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            ShowCrashReporter(ex);
        }
    }

    private static void ShowCrashReporter(Exception? ex)
    {
        if (ex == null) return;

        SentryId sentryId = SentrySdk.CaptureException(ex);
        
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var reporter = new CrashReporter(ex)
            {
                CurrentSentryId = sentryId
            };
            // Use Show instead of ShowDialog if the main window isn't ready or has failed
            if (desktop.MainWindow != null) reporter.ShowDialog(desktop.MainWindow);
            else reporter.Show();
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
