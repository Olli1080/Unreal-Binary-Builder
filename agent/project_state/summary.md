# Project State

## Current Architecture
- **Framework**: Avalonia UI (Cross-platform .NET UI)
- **Design Pattern**: MVVM (Model-View-ViewModel)
- **Dependency Injection**: `Microsoft.Extensions.DependencyInjection`
- **Core Services**:
  - `IProcessExecutor`: Handles external process execution (RunUAT, Setup.bat).
  - `ISettingsService`: Manages application settings, JSON persistence, and log file access.
  - `IPlatformService`: Abstracts OS-specific operations (folder opening, URL launching, shutdown).
  - IUBBUpdater: Handles application updates via NetSparkle.
  - IUnrealEngineProvider: Centralizes UE version detection and path logic.
  - IPluginsService: Manages detection of Epic and custom engine installations.


## Stability & Quality
- **Unit Tests**: 20 passing tests in `UnrealBinaryBuilder.Tests`.
- **Code Style**: CommunityToolkit.Mvvm for ViewModels (ObservableProperties, RelayCommands).
- **Cross-Platform**: Abstracted platform-specific logic into services; UI is platform-agnostic Avalonia.

## Recent Improvements
- Migrated from static `BuilderSettings` to injectable `ISettingsService`.
- Centralized UE knowledge into `IUnrealEngineProvider`.
- Decoupled ViewModels from platform APIs using `IPlatformService`.
