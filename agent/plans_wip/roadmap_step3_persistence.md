# Subplan: Persistence & Build History

## Objective
Provide users with a record of previous build attempts, including durations, logs, and configurations.

## Tasks
- [ ] **History Service**:
  - [ ] Implement `IBuildHistoryService`.
  - [ ] Use `LiteDB` (embedded NoSQL) or a simple JSON array for persistence.
  - [ ] Store: Engine Path, Build Configuration, Duration, Completion Time, Success/Failure, and a reference to the log file.
- [ ] **History Dashboard UI**:
  - [ ] Create a "History" view in the `NavigationView`.
  - [ ] Implement a `DataGrid` or `ListView` to display past builds.
  - [ ] Add "View Log" button to open saved build logs in the editor.
- [ ] **One-Click Rebuild**:
  - [ ] Add "Run Again" button to history items.
  - [ ] Logic: Automatically populate ViewModel with the saved configuration and start the build.

## Validation
- [ ] Build history persists across application restarts.
- [ ] Users can successfully re-run a build using historical parameters.
