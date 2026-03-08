# Subplan: Integration & ViewModel Cleanup

## Objective
Finalize the refactoring by integrating the new services and ensuring robust unit testing.

## Tasks
- [ ] Register `IUIService`, `ISetupService`, `IEngineBuildService`, and `IPluginBuildService` in `App.axaml.cs` (Dependency Injection).
- [ ] Inject all services into `MainWindowViewModel` constructor.
- [ ] Remove extracted logic from `MainWindowViewModel`.
- [ ] Update UI commands in `MainWindowViewModel` to call service methods.
- [ ] Update `MainWindowViewModelTests` to mock all services.
- [ ] Add unit tests for `UIService`, `SetupService`, `EngineBuildService`, and `PluginBuildService`.

## Validation
- [ ] Verify 100% test pass rate for all unit tests.
- [ ] Perform full manual regression test of the application (Build Chain, Engine Build, Plugin Build).
- [ ] Ensure `MainWindowViewModel` line count is significantly reduced (target: <250 lines).
