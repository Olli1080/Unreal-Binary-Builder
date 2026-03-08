# Subplan: Build Orchestration Service Extraction

## Objective
Extract Engine and Plugin build orchestration from `MainWindowViewModel` into specialized services.

## Tasks
- [ ] Define `IEngineBuildService` in `UnrealBinaryBuilder.Avalonia.Classes.Interfaces`.
  - Methods: `BuildEngineAsync`, `PrepareEngineCommandline`.
- [ ] Define `IPluginBuildService` in `UnrealBinaryBuilder.Avalonia.Classes.Interfaces`.
  - Methods: `BuildPluginsAsync`.
- [ ] Implement `EngineBuildService` in `UnrealBinaryBuilder.Avalonia.Classes`.
  - Move `BuildEngine`, `Internal_BuildEngine`, and `Internal_ShutdownPC` logic.
  - Integrate with `PostBuildSettings` for zipping.
- [ ] Implement `PluginBuildService` in `UnrealBinaryBuilder.Avalonia.Classes`.
  - Move `BuildPlugins` logic.
  - Handle plugin-specific zipping logic.
- [ ] Inject these services into `MainWindowViewModel`.

## Validation
- [ ] Verify Engine build successfully invokes `BuildGraph`.
- [ ] Verify Engine zipping after successful build.
- [ ] Verify Plugin build queue processing.
- [ ] Verify individual plugin builds and zipping.
