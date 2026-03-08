# Subplan: UI Modernization & Visual Feedback

## Objective
Enhance the user interface to provide rich, real-time feedback during long-running build processes.

## Tasks
- [x] **Visual Build Stepper**:
  - [x] Implement a `BuildStage` enum to track progress.
  - [x] Map build stages (`Setup`, `Build`, `Zip`) to stepper states in `MainWindowViewModel`.
  - [x] Update `MainWindow.axaml` to display the stepper with dynamic icons and colors.
- [x] **Log Syntax Highlighting**:
  - [x] Create a custom `UnrealLog.xshd` highlighting definition.
  - [x] Implement regex rules for Errors (Red), Warnings (Yellow), and Success (Green).
  - [x] Integrated highlighting into the `TextEditor` instances in `MainWindow`.
- [ ] **Build Statistics Overlay**:
  - [ ] Moved to [Step 3: Persistence & Build History](roadmap_step3_persistence.md) to align with historical tracking.

## Validation
- [x] Logs are visually distinct and easier to scan.
- [x] Users can see exactly which stage the build is in at a glance.
- [x] Verified 100% test pass rate (56/56).
