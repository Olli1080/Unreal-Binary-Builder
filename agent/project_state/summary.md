# Project State

## Current Architecture
- **Framework**: Avalonia UI (Cross-platform .NET UI)
- **Design Pattern**: Service-Oriented MVVM (Lean ViewModels + Specialized Services)
- **Dependency Injection**: `Microsoft.Extensions.DependencyInjection`
- **Core Services**:
  - `IUIService`: UI notifications, message dialogs, navigation, and file/folder picking.
  - `ISetupService`: Orchestration of Engine setup (`Setup.bat`, project generation).
  - `IEngineBuildService`: Engine builds via `BuildGraph`.
  - `IPluginBuildService`: Plugin build queue management via `UAT`.
  - `IZipService`: Complex filtering and zipping logic for artifacts.
  - `IGitService`: Extraction of repository metadata (branches, hashes).
  - `IBuildTimerService`: High-precision build timing and UI updates.
  - `ILogFormatterService`: Regex-based compilation progress and log processing.
  - `IProcessExecutor`: Handles external process execution.
  - `ISettingsService`: Manages application settings and JSON persistence.
  - `IPlatformService`: Abstracts OS-specific operations.
  - `IUBBUpdater`: Handles application updates via NetSparkle.
  - `IUnrealEngineProvider`: Centralizes UE version detection and path logic.


## Stability & Quality
- **Unit Tests**: 56 passing tests in `UnrealBinaryBuilder.Tests`, achieving modular coverage for all core services.
- **Code Style**: CommunityToolkit.Mvvm for ViewModels; logic isolated in testable, injectable services.
- **Cross-Platform**: Platform-agnostic core logic and UI (Avalonia).

## Recent Improvements
- **God ViewModel Decomposition**: Successfully refactored `MainWindowViewModel` from a 500+ line "God Object" into specialized services, reducing it to a lean ~230 lines focused on UI state.
- **Modular Testing Suite**: Implemented 36+ new unit tests for individual services, ensuring robust business logic in isolation.
- **Logic Parity & Safety**: Verified 100% logic parity during refactor, including improved null-safety and locale-independent string formatting.
