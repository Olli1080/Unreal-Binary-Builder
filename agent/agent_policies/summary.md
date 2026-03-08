# Agent Policies

The following policies apply to this project:

- **Architecture**: Strict adherence to a **Service-Oriented MVVM** pattern.
  - ViewModels MUST be lean (~250 lines or less) and focus exclusively on UI state and command delegation.
  - Business logic (Build orchestration, Git metadata, Zipping, Setup) MUST reside in specialized, injectable services.
  - Use `CommunityToolkit.Mvvm` for ViewModel implementation.
- **Dependency Injection**: Use `Microsoft.Extensions.DependencyInjection`. All services MUST be registered in `App.axaml.cs` and injected into ViewModels via constructor injection.
- **UI Framework**: Avalonia UI targeting cross-platform desktop.
- **UI Interactions**: All UI-specific tasks (Toasts, Message Dialogs, File/Folder Picking, Clipboard, Navigation) MUST be routed through the `IUIService`. This ensures ViewModels remain decoupled from the UI framework and are fully testable.
- **Modular Testing**: Every new service MUST have a corresponding unit test suite in `UnrealBinaryBuilder.Tests`.
  - Services should be tested in isolation using mocks for their dependencies.
  - ViewModels MUST be tested using mocks for all injected services.
  - Maintain 100% pass rate and prioritize high coverage for business logic.
- **Robustness & Locale Safety**: 
  - All numeric-to-string conversions (e.g., thread counts, multipliers) MUST use `CultureInfo.InvariantCulture` to prevent locale-specific formatting issues (e.g., `,` vs `.` decimal separators).
  - Use `PathHelpers` for consistent path normalization across platforms.
- **Background Processing**: All shell executions MUST use the `IProcessExecutor` service. It automatically handles logging to the `IUBBLogger` system.
- **Logging & Observability**: All system, build, and telemetry events MUST be routed through the `IUBBLogger` service.
- **Zipping & Artifacts**: All zipping operations MUST use the `IZipService` to ensure consistent filtering rules (e.g., PDB/Source exclusions) and proper progress reporting via `ZipProgress`.
- **Versioning**: Follow semantic versioning and maintain the separated `CHANGELOG.md` system in the `changelogs/` folder.
- **Target Framework**: .NET 10.0.

## Platform-Specific Policies
- [Windows Policies](windows.md)
