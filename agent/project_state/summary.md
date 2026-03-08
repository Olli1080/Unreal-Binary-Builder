# Project State

## Current Architecture
- **Framework**: Avalonia UI (Cross-platform .NET 10.0)
- **Design Pattern**: Service-Oriented MVVM (Lean ViewModels + Specialized Services)
- **Dependency Injection**: `Microsoft.Extensions.DependencyInjection`
- **Build System**: GitHub Actions (Win/Linux/macOS)
- **Persistence**: JSON-based storage for Settings, Build History, and Orchestration State.

## Core Services
- `IEngineBuildService`: Orchestrates the Unreal BuildGraph.
- `ISetupService`: Manages Git dependency setup and prerequisites.
- `IPluginBuildService`: Handles batch plugin compilation.
- `IBuildHistoryService`: Archives past build metadata and logs.
- `IBuildOrchestrationService`: Tracks current build progress for resume capabilities.
- `IProcessExecutor`: Cross-platform shell execution with `.sh`/`.bat` parity.
- `IUBBLogger`: Unified logging with multiple sinks (UI, File, Telemetry).

## Recent Improvements
- **CI/CD & Cross-Platform Validation**: Automated verification on all 3 major OS platforms with portable zip artifact generation.
- **UI Modernization**: Real-time Visual Build Stepper and custom `AvaloniaEdit` syntax highlighting for Unreal logs.
- **Persistence & History**: Full record of past builds with "One-Click Rebuild" and archived log viewing.
- **Advanced Orchestration**: Intelligent build resume logic allowing recovery from failed stages (e.g., skip setup if already done).
- **Quality Assurance**: 56 unit tests ensuring robust business logic across all services.

## Logic Parity & Safety
- 100% logic parity maintained during WPF to Avalonia migration.
- Improved null-safety and platform-neutral path handling using `PathHelpers`.
