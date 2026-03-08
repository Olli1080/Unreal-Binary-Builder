# Subplan: Advanced Orchestration

## Objective
Increase build efficiency and error recovery by allowing the system to intelligently resume from failed or partially completed stages.

## Tasks
- [ ] **Stateful Build Tracker**:
  - [ ] Implement a system to mark individual build stages as "Complete" in a persistent file (`.ubb_state`).
  - [ ] Example: If `Internal_BuildEngine` succeeds but `ZipService` fails, the state should reflect that the compilation is done.
- [ ] **Smart Resume Logic**:
  - [ ] Before starting a build, check for existing `.ubb_state`.
  - [ ] If state exists, ask the user: "Partial build detected. Resume from [Zipping]?"
- [ ] **Dirty State Detection**:
  - [ ] Invalidate state if the Engine Path or Git Hash changes.

## Validation
- [ ] Failing at the "Zipping" stage allows the user to re-run zipping only without recompiling the entire engine.
