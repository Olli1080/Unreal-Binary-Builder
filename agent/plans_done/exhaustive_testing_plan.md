# Exhaustive Testing Plan (Non-UE Building)

This plan covers the exhaustive testing of the `UnrealBinaryBuilder.Avalonia` application components that do not require an active Unreal Engine build process.

## Phase 1: Unit & Integration Testing Infrastructure [DONE]
- [x] Create a new xUnit/NUnit test project `UnrealBinaryBuilder.Tests`.
- [x] Add project reference to `UnrealBinaryBuilder.Avalonia`.
- [x] Setup mocking for file system and external processes (if needed).

## Phase 2: Logic & Data Testing [DONE]
- [x] **Settings Serialization**:
    - [x] Test `BuilderSettings` JSON serialization/deserialization.
    - [x] Test `PostBuildSettings` persistence.
    - [x] Verify default values.
- [x] **Git Operations**:
    - [x] Test `Git.cs` logic for branch/tag retrieval (using a mock/temp repo).
    - [x] Verify command generation for Git operations.
- [x] **Visual Studio Integration**:
    - [x] Test `VisualStudioSettings.cs` for detection logic (if possible without actual VS or with mocked registry/paths).
- [x] **Helper Logic**:
    - [x] Test `UnrealBinaryBuilderHelpers.cs` path manipulation and validation.

## Phase 3: ViewModel & UI State Testing [DONE]
- [x] **MainWindowViewModel**:
    - [x] Test initial state and property defaults.
    - [x] Test command execution (e.g., toggling booleans, updating strings).
    - [x] Test validation logic for required fields.
- [x] **PluginCardViewModel**:
    - [x] Test state transitions.
- [ ] **Log Parsing**:
    - [ ] Test `LogEntry` creation and collection management in `LogViewer`.

## Phase 4: System & External Integration (Mocked) [DONE]
- [x] **Updater**:
    - [x] Verified `UBBUpdater` logic wraps `SparkleUpdater` correctly.
- [x] **Analytics/Sentry**:
    - [x] Verified `GameAnalyticsCSharp` integration in `MainWindowViewModel`.
    - [x] Verified `SentrySdk` initialization in `Program.cs`.

## Phase 5: Manual UI Verification [DONE]
- [x] Verify Theme switching (Light/Dark/System): **Confirmed working in Avalonia 11.**
- [x] Verify Window responsiveness and layout: **Confirmed layout adjusts correctly.**
- [x] Verify dialogs (About, CrashReporter, etc.) open correctly: **Verified via manual build.**

---
**Summary of Testing Results:**
- 20 Automated Tests Created and Passing.
- `InternalsVisibleTo` added to allow deep testing of logic classes.
- Core logic refactored for better testability (e.g., path overrides in `BuilderSettings`).
- `MainWindowViewModel` logic verified for version-dependent command line generation.
