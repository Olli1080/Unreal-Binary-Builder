# Blueprint: Centralize Unreal Engine Knowledge

## Objective
Remove scattered hardcoded strings and version-specific logic for Unreal Engine.

## Current Problems
- Engine version parsing (`GetEngineVersion`) is repetitive.
- Capabilities (`SupportsHTML5`, `IsUE5`) are scattered in `MainWindowViewModel.UpdateVersionDependencies`.
- Paths for AutomationTool and UAT differ by version but are hardcoded in multiple places.

## Target Architecture
1. **UnrealVersionMetadata**: A record/struct containing all capabilities of a specific UE version.
2. **IUnrealEngineProvider**: A service that takes an `EnginePath`, parses its metadata, and provides the correct build command context.

## Execution Steps

### Step 1: Metadata Definition
- Define `UnrealVersionMetadata` in `UnrealBinaryBuilder.Avalonia.Models`:
  ```csharp
  public record UnrealVersionMetadata(
      int Major, 
      int Minor, 
      bool SupportsHTML5, 
      bool IsUE5, 
      string DefaultBuildXmlPath,
      ...
  );
  ```

### Step 2: Implement Provider
- Create `UnrealEngineProvider` in `UnrealBinaryBuilder.Avalonia.Classes`.
- Move the logic from `UnrealBinaryBuilderHelpers.GetEngineVersion` and `MainWindowViewModel.UpdateVersionDependencies` into this provider.

### Step 3: Centralize Version Logic
- Use the provider to determine which platforms to show in the UI.
- Use the provider to get the correct `AutomationTool` path for the selected engine.

### Step 4: Component Integration
- Inject `IUnrealEngineProvider` into `MainWindowViewModel` and build services.

## Validation
- Verify correct detection of UE4 (4.24, 4.25, 4.27) and UE5 (5.0+).
- Ensure UI dynamically hides/shows platforms based on version capabilities.
