# Blueprint: Refactor God ViewModel (MainWindowViewModel)

## Objective
Decompose `MainWindowViewModel.cs` from a 500+ line "God Object" into specialized, single-responsibility services.

## Current Problems
- Handles UI state, file picking, build orchestration, git logic, and timers.
- Difficult to unit test build logic without a full UI context.
- High risk of regression when changing unrelated features.

## Target Architecture
1. **IEngineBuildService**: Handles the orchestration of Engine builds (BuildGraph).
2. **IPluginBuildService**: Handles the orchestration of Plugin builds (UAT).
3. **ISetupService**: Handles `Setup.bat` and `GenerateProjectFiles.bat` execution.
4. **IUIService**: Manages navigation, toast notifications, and dialogs.

## Execution Steps

### Step 1: Interface Definition
- Define interfaces for the new services in `UnrealBinaryBuilder.Avalonia.Classes.Interfaces`.

### Step 2: Extract Build Services
- Move `Internal_BuildEngine` and `PrepareCommandline` to `EngineBuildService`.
- Move `BuildPlugins` logic to `PluginBuildService`.
- Move `StartSetup` logic to `SetupService`.

### Step 3: Extract UI Utilities
- Move `ShowToast`, `ShowMessageDialog`, and `ApplyTheme` to a `UIService` or keep in a leaner `MainWindowViewModel`.

### Step 4: Component Integration
- Inject the new services into `MainWindowViewModel` via constructor (DI).
- Update the ViewModel to delegate build commands to these services.

## Validation
- Ensure unit tests for `MainWindowViewModel` can now use mocks for build services.
- Verify build workflows still function correctly from the UI.
