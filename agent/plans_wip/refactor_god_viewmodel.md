# Blueprint: Refactor God ViewModel (MainWindowViewModel)

## Objective
Decompose `MainWindowViewModel.cs` from a 500+ line "God Object" into specialized, single-responsibility services.

## Current Problems
- Handles UI state, file picking, build orchestration, git logic, and timers.
- Difficult to unit test build logic without a full UI context.
- High risk of regression when changing unrelated features.

## Target Architecture
1. **IUIService**: UI notifications, message dialogs, and navigation.
2. **ISetupService**: Engine setup (Setup.bat, GenerateProjectFiles.bat).
3. **IEngineBuildService**: Engine builds (BuildGraph orchestration).
4. **IPluginBuildService**: Plugin builds (UAT orchestration).

## Execution Subplans
- [x] [Step 1: UI Service Extraction](refactor_god_viewmodel_step1_ui.md)
- [ ] [Step 2: Engine Setup Service Extraction](refactor_god_viewmodel_step2_setup.md)
- [ ] [Step 3: Build Orchestration Service Extraction](refactor_god_viewmodel_step3_build.md)
- [ ] [Step 4: Integration & ViewModel Cleanup](refactor_god_viewmodel_step4_integration.md)

## Validation Strategy
- **Unit Testing**: All extracted services must have unit tests. `MainWindowViewModel` must have its dependencies mocked.
- **Regression Testing**: All build workflows (Setup Chain, Engine Build, Plugin Build) must be manually verified.
- **Complexity Analysis**: `MainWindowViewModel` should be reduced to UI-state management only, with a line count under 250.
