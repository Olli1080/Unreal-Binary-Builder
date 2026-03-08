# Plans WIP

The following architectural blueprints are ready for execution:

- [refactor_god_viewmodel.md](refactor_god_viewmodel.md): Plan to decompose `MainWindowViewModel` into specialized services.
  - [x] [Step 1: UI & Dialog Service Extraction](refactor_god_viewmodel_step1_ui.md)
  - [ ] [Step 2: Engine Setup Service Extraction](refactor_god_viewmodel_step2_setup.md)
  - [ ] [Step 3: Build Orchestration Service Extraction](refactor_god_viewmodel_step3_build.md)
  - [ ] [Step 4: Integration & ViewModel Cleanup](refactor_god_viewmodel_step4_integration.md)

## Future Roadmap (Uncharted)

- **UI Modernization**: 
  - Improving the visual feedback during long build processes.
  - Adding a more comprehensive "Dashboard" view for recent builds.
- **Publish & Deployment**: 
  - Verifying shell execution on Linux and macOS environments.
  - Setting up a cross-platform build pipeline via GitHub Actions.
  - Creating distribution packages (AppImage, deb, .app) for non-Windows platforms.
