# Roadmap: Modernization, CI/CD & Persistence

## Objective
Transition the project from a stable, refactored state to a production-ready, cross-platform application with modern UI feedback and historical tracking.

## Phase Overview

### [Step 1: CI/CD & Cross-Platform Validation](roadmap_step1_cicd.md)
- **Goal**: Ensure the "cross-platform" architecture actually works on Linux and macOS.
- **Key Tasks**: GitHub Actions integration, shell parity verification (`.sh` vs `.bat`).

### [Step 2: UI Modernization & Visual Feedback](roadmap_step2_ui.md)
- **Goal**: Improve the user experience during long builds.
- **Key Tasks**: Visual build stepper, `AvaloniaEdit` syntax highlighting for logs.

### [Step 3: Persistence & Build History](roadmap_step3_persistence.md)
- **Goal**: Prevent data loss between sessions.
- **Key Tasks**: SQLite/JSON build history dashboard, one-click rebuild logic.

### [Step 4: Advanced Orchestration](roadmap_step4_orchestration.md)
- **Goal**: Efficient error recovery.
- **Key Tasks**: Stateful build tracking, resume from failed stage.

## Success Criteria
- [ ] 100% test pass rate on GitHub Actions (Win/Linux/macOS).
- [ ] Visual indication of build progress beyond simple text.
- [ ] Ability to view previous build logs from a dashboard.
