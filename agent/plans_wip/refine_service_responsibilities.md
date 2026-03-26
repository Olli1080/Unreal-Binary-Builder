# Refine Service Responsibilities

This plan aims to further thin out the `MainWindowViewModel` and clarify the roles of various services.

## Goals
- Reduce the size and complexity of `MainWindowViewModel`.
- Create a dedicated application initialization flow.
- Formalize the build pipeline as a sequence of distinct, manageable steps.

## Tasks
- [x] **App Initialization**:
    - Create `IAppInitializer` to handle startup tasks (Update checks, Telemetry init, Theme loading).
    - Move initialization logic out of `MainWindowViewModel` constructor.
- [x] **Build Pipeline Orchestration**:
    - Refactor `EngineBuildService` to focus strictly on the build.
    - Create `IBuildPipeline` to coordinate Setup -> Build -> Zip -> Post-Build actions.
    - Implement progress reporting that spans the entire pipeline.
- [ ] **Settings Management**:
    - Ensure `ISettingsService` is the single source of truth for all persistence.
