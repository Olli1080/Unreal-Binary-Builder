# Agent Policies

The following policies apply to this project:

- **Architecture**: Strict adherence to the Model-View-ViewModel (MVVM) pattern using `CommunityToolkit.Mvvm`.
- **UI Framework**: Avalonia UI targeting cross-platform desktop (Windows, macOS, Linux).
- **Styling**: Use `FluentAvalonia` for all modern UI components, dialogs, and theming (Light/Dark/System).
- **Code Editing**: Use `AvaloniaEdit` (`Avalonia.AvaloniaEdit`) for integrated code and log viewing.
- **Background Processing**: All shell executions (like Unreal's `AutomationTool`) MUST use the `IProcessExecutor` service via DI. It automatically handles logging of standard output/error to the `IUBBLogger` system.
- **System Interactions**: Use Avalonia's `StorageProvider` for all file/folder picking dialogs (do not use Windows Forms or legacy WPF dialogs). Use `Process.Start` with `UseShellExecute = true` for opening URLs or folders.
- **Logging & Observability**: All system, build, and telemetry events MUST be routed through the `IUBBLogger` service. Use appropriate `LogLevel` and `LogCategory` for each event.
  - `UiLogSink`: Handles real-time log updates to the ViewModel.
  - `FileLogSink`: Manages local log file persistence.
  - `TelemetryLogSink`: Automatically routes warnings and errors to GameAnalytics.
- **Updates**: Use `NetSparkleUpdater.UI.Avalonia` for seamless cross-platform updates. The legacy WPF `UnrealBinaryBuilderUpdater` CLI is considered obsolete.
- **Versioning**: Follow semantic versioning and maintain the separated `CHANGELOG.md` system in the `changelogs/` folder.
- **Target Framework**: .NET 10.0.

## Platform-Specific Policies
- [Windows Policies](windows.md)
