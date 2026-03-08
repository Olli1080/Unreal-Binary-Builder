# Agent Policies

The following policies apply to this project:

- **Architecture**: Strict adherence to the Model-View-ViewModel (MVVM) pattern using `CommunityToolkit.Mvvm`.
- **UI Framework**: Avalonia UI targeting cross-platform desktop (Windows, macOS, Linux).
- **Styling**: Use `FluentAvalonia` for all modern UI components, dialogs, and theming (Light/Dark/System).
- **Code Editing**: Use `AvaloniaEdit` (`Avalonia.AvaloniaEdit`) for integrated code and log viewing.
- **Background Processing**: All shell executions (like Unreal's `AutomationTool`) MUST use the `ProcessExecutor` class to ensure asynchronous, non-blocking standard output/error stream reading via `WaitForExitAsync`.
- **System Interactions**: Use Avalonia's `StorageProvider` for all file/folder picking dialogs (do not use Windows Forms or legacy WPF dialogs). Use `Process.Start` with `UseShellExecute = true` for opening URLs or folders.
- **Observability**: Maintain full event parity with the original WPF app using `GameAnalytics`. All unhandled exceptions must trigger the `SentrySdk` and display the `CrashReporter` dialog.
- **Updates**: Use `NetSparkleUpdater.UI.Avalonia` for seamless cross-platform updates. The legacy WPF `UnrealBinaryBuilderUpdater` CLI is considered obsolete.
- **Versioning**: Follow semantic versioning and maintain the separated `CHANGELOG.md` system in the `changelogs/` folder.
- **Target Framework**: .NET 10.0.

## Platform-Specific Policies
- [Windows Policies](windows.md)
