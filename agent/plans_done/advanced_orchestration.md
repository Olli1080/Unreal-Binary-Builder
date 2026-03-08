# Subplan: Advanced Orchestration

## Objective
Increase build efficiency and error recovery by allowing the system to intelligently resume from failed or partially completed stages.

## Tasks
- [x] **Stateful Build Tracker**:
  - [x] Implement `IBuildOrchestrationService` to mark individual build stages as "Complete".
  - [x] Persist build state in `CurrentBuildState.json`.
  - [x] Track Engine Path, Git Hash, and Settings to ensure state validity.
- [x] **Smart Resume Logic**:
  - [x] Check for existing build state before starting a fresh build.
  - [x] Prompt user to resume if a valid partial build is detected.
  - [x] Support resuming from "Setup" (skips to Engine Build) and "Build" (skips to Packaging).
- [x] **Dirty State Detection**:
  - [x] Invalidate state if the Engine Path, Git Hash, or Build Settings change.

## Validation
- [x] Failing at the "Zipping" stage allows the user to re-run and resume from zipping only without recompiling the entire engine.
- [x] Verified 100% test pass rate (56/56).
