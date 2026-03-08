# Subplan: Engine Setup Service Extraction

## Objective
Move the Engine setup logic (Setup.bat, GenerateProjectFiles.bat, AutomationTool build) from `MainWindowViewModel` to a specialized `ISetupService`.

## Tasks
- [x] Define `ISetupService` in `UnrealBinaryBuilder.Avalonia.Classes.Interfaces`.
  - Methods: `RunSetupChain`, `RunSetup`, `GenerateProjectFiles`, `BuildAutomationTool`.
- [x] Implement `SetupService` in `UnrealBinaryBuilder.Avalonia.Classes`.
  - Inject `IProcessExecutor`, `IUBBLogger`, and `ISettingsService`.
- [x] Refactor `StartSetup` command in `MainWindowViewModel` to delegate to `ISetupService`.
- [x] Move `SetupBatCommandLineArgs` logic into `SetupService`.

## Validation
- [x] Verify `Setup.bat` runs correctly with arguments (via Unit Test).
- [x] Verify `GenerateProjectFiles.bat` is invoked successfully.
- [x] Verify `AutomationTool.sln` is built correctly via MSBuild.
- [x] Check logs for proper reporting of each step.
