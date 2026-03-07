# Granular Migration Plan: WPF to Avalonia UI

This plan tracks the specific porting tasks for moving the Unreal-Binary-Builder to the Avalonia UI (.NET 10.0) framework.

## 1. ViewModels & Business Logic (Mvvm Integration)
- [x] **Core Logic Migration**
  - [x] Port `BuilderSettings.cs` to be MVVM compatible
  - [x] Port `Git.cs` (LibGit2Sharp interaction)
  - [x] Port `Plugins.cs` (Plugin discovery and management)
  - [x] Port `VisualStudioSettings.cs`
  - [x] Port `PostBuildSettings.cs`
- [x] **Main Logic Migration**
  - [x] Extract build orchestration logic from `MainWindow.xaml.cs`
  - [x] Implement command handling in `MainWindowViewModel`
  - [x] Implement build progress reporting (Events/Observables)
  - [x] Implement build timing (Stopwatch/Timer)

## 2. UserControls & UI Components
- [x] **Porting Individual Controls**
  - [x] `PluginCard`: Port XAML and logic to Avalonia UserControl
  - [x] `AboutDialog`: Port to Avalonia Window with Fluent styles
  - [x] `LogViewer`: Re-implement using `AvaloniaEdit` and MVVM binding
  - [x] `CrashReporter`: Port Sentry-integrated reporting UI
  - [x] `DownloadDialog`: Port browser/download UI (replacing CefSharp if necessary)

- [x] **Main Window Layout**
  - [x] Finalize `NavigationView` structure
  - [x] Port "Engine Builder" tab content
  - [x] Port "Plugin Builder" tab content
  - [x] Port "Settings" area
  - [x] Integrate Toast Notifications (WindowNotificationManager)

## 3. Infrastructure & Cross-Platform Support
- [x] **System Interactions**
  - [x] Implement cross-platform File/Folder pickers (Avalonia `StorageProvider`)
  - [x] Implement cross-platform Process execution (for UBT/AutomationTool)
  - [x] Verify shell execution on Windows/Linux/macOS
- [x] **Analytics & Crash Reporting**
  - [x] Port Sentry integration
  - [x] Port GameAnalytics integration (Full Event Parity)
- [x] **Updater Migration**
  - [x] Port `UnrealBinaryBuilderUpdater` logic
  - [x] Verify Sparkle/NetSparkle compatibility with Avalonia

## 4. Final Polish & Deployment
- [x] Implement Theme switching (Light/Dark/System)
- [x] Setup Cross-platform build pipeline (GitHub Actions)
- [ ] Create distribution packages (Windows .exe, Linux AppImage/deb, macOS .app)
