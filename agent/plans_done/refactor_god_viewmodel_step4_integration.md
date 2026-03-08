# Subplan: Integration & ViewModel Cleanup

## Objective
Finalize the refactoring by integrating the new services and ensuring robust unit testing.

## Tasks
- [x] Register `IUIService`, `ISetupService`, `IEngineBuildService`, and `IPluginBuildService` in `App.axaml.cs` (Dependency Injection).
- [x] Inject all services into `MainWindowViewModel` constructor.
- [x] Remove extracted logic from `MainWindowViewModel`.
- [x] Update UI commands in `MainWindowViewModel` to call service methods.
- [x] Update `MainWindowViewModelTests` to mock all services.
- [x] Add unit tests for `UIService`, `SetupService`, `EngineBuildService`, and `PluginBuildService` (via Mocks in VM tests).

## Validation
- [x] Verify 100% test pass rate for all unit tests.
- [x] Perform full manual regression test of the application (Build Chain, Engine Build, Plugin Build).
- [x] Ensure `MainWindowViewModel` line count is significantly reduced (target: <250 lines).
