# Subplan: Engine Setup Service Extraction

## Objective
Move the Engine setup logic (Setup.bat, GenerateProjectFiles.bat, AutomationTool build) from `MainWindowViewModel` to a specialized `ISetupService`.

## Tasks
- [ ] Define `ISetupService` in `UnrealBinaryBuilder.Avalonia.Classes.Interfaces`.
  - Methods: `RunSetupChain`, `RunSetup`, `GenerateProjectFiles`, `BuildAutomationTool`.
- [ ] Implement `SetupService` in `UnrealBinaryBuilder.Avalonia.Classes`.
  - Inject `IProcessExecutor`, `IUBBLogger`, and `ISettingsService`.
- [ ] Refactor `StartSetup` command in `MainWindowViewModel` to delegate to `ISetupService`.
- [ ] Move `SetupBatCommandLineArgs` logic into `SetupService`.

## Validation
- [ ] Verify `Setup.bat` runs correctly with arguments.
- [ ] Verify `GenerateProjectFiles.bat` is invoked successfully.
- [ ] Verify `AutomationTool.sln` is built correctly via MSBuild.
- [ ] Check logs for proper reporting of each step.
