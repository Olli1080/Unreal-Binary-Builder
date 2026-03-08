# Plan: Avalonia UI Migration (Phase 3)

## Objective
Migrate the WPF application to Avalonia UI (.NET 10.0) for cross-platform support.

## 1. ViewModels & Business Logic (COMPLETED)
- [x] Port `BuilderSettings.cs` to be MVVM compatible
- [x] Port `Git.cs` (LibGit2Sharp interaction)
- [x] Port `Plugins.cs` (Plugin discovery and management)
- [x] Port `VisualStudioSettings.cs`
- [x] Port `PostBuildSettings.cs`
- [x] Extract build orchestration logic from `MainWindow.xaml.cs`
- [x] Implement command handling in `MainWindowViewModel`
- [x] Implement build progress reporting (Events/Observables)
- [x] Implement build timing (Stopwatch/Timer)

## 2. UserControls & UI Components (COMPLETED)
- [x] Port `PluginCard`: Port XAML and logic to Avalonia UserControl
- [x] Port `AboutDialog`: Port to Avalonia Window with Fluent styles
- [x] Port `LogViewer`: Re-implement using `AvaloniaEdit` and MVVM binding
- [x] Port `CrashReporter`: Port Sentry-integrated reporting UI
- [x] Port `DownloadDialog`: Port browser/download UI
- [x] Finalize `NavigationView` structure
- [x] Port "Engine Builder" tab content
- [x] Port "Plugin Builder" tab content
- [x] Port "Settings" area
- [x] Integrate Toast Notifications (WindowNotificationManager)
- [x] Replace `HandyControl` with `FluentAvalonia` equivalents

## 3. Infrastructure & Cross-Platform Support (COMPLETED)
- [x] Implement cross-platform File/Folder pickers (Avalonia `StorageProvider`)
- [x] Implement cross-platform Process execution (for UBT/AutomationTool)
- [x] Verify shell execution on Windows
- [x] Port Sentry integration
- [x] Port GameAnalytics integration (Full Event Parity)
- [x] Port `UnrealBinaryBuilderUpdater` logic
- [x] Verify Sparkle/NetSparkle compatibility with Avalonia

## 4. Final Polish & Deployment (COMPLETED)
- [x] Implement Theme switching (Light/Dark/System)
- [x] Resolve `AvaloniaEdit` assembly loading for the final build
- [x] Setup Initial build pipeline (GitHub Actions)
