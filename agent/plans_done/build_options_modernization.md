# Build Options Modernization

This plan focuses on expanding the available build configurations based on options discovered in modern Unreal Engine metadata (`meta.json`).

## Goals
- Expose new target platforms (WinArm64, VisionOS, etc.).
- Provide control over build performance (Parallel Executor).
- Add granular control for extra compiler and DDC arguments.
- Support advanced metadata and publishing options.

## Tasks
- [x] **Step 1: Model Updates**
    - [x] Add new boolean properties to `BuilderSettingsJson.cs` (`WithWinArm64`, `WithWinArm64ec`, `WithVisionOS`, `AllowParallelExecutor`, `SignWindowsExecutablesInParallel`, `IncludeDocs`, `AllPlatforms`).
    - [x] Add new string properties to `BuilderSettingsJson.cs` (`ExtraCompileArgs`, `ExtraCompileArgsMac`, `ExtraDDCArgs`, `BuildIdOverride`).
- [x] **Step 2: Engine Metadata Detection**
    - [x] Update `UnrealEngineMetadata.cs` to include properties for new platform support.
    - [x] Update `UnrealEngineProvider.cs` to detect support for VisionOS and WinArm64 (based on version logic).
- [x] **Step 3: Argument Generation**
    - [x] Update `BuildArgumentBuilder.cs` to map the new settings to their respective BuildGraph flags (e.g., `-set:AllowParallelExecutor=true`).
- [x] **Step 4: ViewModel & UI**
    - [x] Add observable properties to `MainWindowViewModel.cs` for UI state (e.g., `SupportVisionOS`).
    - [x] Update `MainWindow.axaml` to include new checkboxes in the "Platforms" section.
    - [x] Create an "Advanced Arguments" section in the UI for the new string fields and executor settings.
- [x] **Step 5: Verification**
    - [x] Update `UnrealEngineIntegrationTests.cs` to verify the new flags are generated correctly for newer engine versions.
    - [x] Ensure backward compatibility with older engine versions (where these flags might not exist).
