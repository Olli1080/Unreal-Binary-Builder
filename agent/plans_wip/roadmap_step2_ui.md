# Subplan: UI Modernization & Visual Feedback

## Objective
Enhance the user interface to provide rich, real-time feedback during long-running build processes.

## Tasks
- [ ] **Visual Build Stepper**:
  - [ ] Implement a `Stepper` control or use a styled `ItemsControl`.
  - [ ] Map build stages (`Setup`, `Engine Build`, `Zipping`) to stepper states (Idle, Running, Completed, Failed).
  - [ ] Update `MainWindow` to display the stepper at the top of the build area.
- [ ] **Log Syntax Highlighting**:
  - [ ] Create a custom `IHighlightingDefinition` for Unreal Engine logs.
  - [ ] Highlighting rules:
    - `[ERROR]` and compilation errors in Red.
    - `[WARNING]` in Yellow.
    - Success messages and `[Compiled: X/Y]` in Green.
  - [ ] Integrate highlighting into the `LogViewer` control.
- [ ] **Build Statistics Overlay**:
  - [ ] Show a summary overlay upon completion (Duration, Total Files, Peak Memory, etc.).

## Validation
- [ ] Logs are visually distinct and easier to scan.
- [ ] Users can see exactly which stage the build is in at a glance.
