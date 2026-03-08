# Plan: Modular Service Testing

## Objective
Leverage the newly decoupled architecture to implement exhaustive unit tests for every specialized service. This ensures that business logic is verified in isolation from the UI.

## Phase 1: Setup Service Verification
- [x] Create `SetupServiceTests.cs`.
- [x] Test `PrepareSetupArgs` with:
  - [x] All platforms included vs. specific exclusions.
  - [x] Cache enabled/disabled and custom cache paths.
  - [x] Proxy and thread count variations.
- [x] Test `RunSetupChainAsync` for:
  - [x] Full chain success.
  - [x] Failure at `Setup.bat` (should stop chain).
  - [x] Failure at `GenerateProjectFiles` (should stop chain).
  - [x] Failure at `BuildAutomationTool` (should stop chain).

## Phase 2: Zip Service Filtering Logic
- [x] Create `ZipServiceTests.cs`.
- [x] Mock file system to verify filtering logic:
  - [x] Inclusion/Exclusion of PDBs and Debug files.
  - [x] Correct handling of Source vs. Runtime vs. Developer folders.
  - [x] Feature Pack and Template filtering.
  - [x] Plugin-specific marketplace zipping (excluding Binaries/Intermediate).

## Phase 3: Log & UI Interaction
- [x] Create `LogFormatterServiceTests.cs`.
  - [x] Test regex matching for various compiler outputs.
  - [x] Verify total vs. current file count tracking.
- [x] Expand `UIService` verification (via `MainWindowViewModelTests`).

## Phase 4: Build Orchestration
- [x] Create `EngineBuildServiceTests.cs`.
- [x] Create `PluginBuildServiceTests.cs`.
  - [x] Test queue processing logic.
  - [x] Verify that failing one plugin in the queue stops or continues based on requirements.

## Validation
- [x] Achieve >90% code coverage for all `Classes/` logic.
- [x] Maintain 100% pass rate across the entire suite (56 tests).
