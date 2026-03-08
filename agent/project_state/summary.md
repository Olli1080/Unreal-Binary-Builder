# Project Status Summary

## Current State
- **UI Framework**: Avalonia UI (targeting .NET 10.0)
- **Primary Platform**: Windows (win32) with cross-platform readiness.
- **Main Components**:
    - `UnrealBinaryBuilder.Avalonia`: Modern cross-platform UI.
    - `UnrealBinaryBuilderUpdater`: Active updater project.
    - `UnrealBinaryBuilder.Tests`: Automated xUnit testing suite (20 tests passing).
- **Recent Progress**:
    - **Standardized Path Normalization**: Introduced `PathHelpers` for consistent Unix-style path handling (crucial for Unreal Engine CLI compatibility).
    - **Project Sanitization**: Archived legacy WPF project to `archive/` to reduce codebase noise and focus on Avalonia.
    - **AI Guidance**: Implemented `.geminiignore` to focus agent context on active code only.
    - **Refined Testing**: Updated all 20 unit tests to verify path normalization and cross-platform logic.

## Testing Results
- 20 unit/integration tests successfully verified:
    - Path normalization (Unix vs Windows separators).
    - Settings serialization and persistence.
    - Git branch/hash detection logic.
    - Visual Studio version and MSBuild path discovery.
    - Engine version extraction and version-dependent feature toggling.
    - BuildGraph command-line argument generation for both UE4 and UE5.
    - Plugin data loading and build state management.
    - Post-build ZIP file generation and skip-filtering logic.

## Next Steps (Phase 2: Modernization)
- **Decompose MainWindowViewModel**: Extract build logic into specialized services.
- **Centralize Engine Knowledge**: Replace scattered version checks with `IUnrealEngineProvider`.
- **Unified Logging**: Implement `IUBBLogger` with multi-sink support.
- **Dependency Injection**: Adopt `Microsoft.Extensions.DependencyInjection`.
- **Strong-Typed Build Config**: Replace string concatenation with `BuildArgumentBuilder`.
