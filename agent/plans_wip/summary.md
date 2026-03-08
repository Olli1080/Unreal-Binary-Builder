# Plans WIP

The following architectural blueprints are ready for execution:

- [refactor_god_viewmodel.md](refactor_god_viewmodel.md): Plan to decompose `MainWindowViewModel` into specialized services.
- [unified_logging_strategy.md](unified_logging_strategy.md): Plan to implement a centralized logging service with UI and file sinks.

## Future Roadmap (Uncharted)

- **UI Modernization**: 
  - Improving the visual feedback during long build processes.
  - Adding a more comprehensive "Dashboard" view for recent builds.
- **Publish & Deployment**: 
  - Verifying shell execution on Linux and macOS environments.
  - Setting up a cross-platform build pipeline via GitHub Actions.
  - Creating distribution packages (AppImage, deb, .app) for non-Windows platforms.
