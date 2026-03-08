# Plans WIP

The following architectural blueprints are ready for execution:

- [refactor_god_viewmodel.md](refactor_god_viewmodel.md): Plan to decompose `MainWindowViewModel` into specialized services.
- [centralize_engine_knowledge.md](centralize_engine_knowledge.md): Plan to consolidate UE-specific metadata and path logic.
- [unified_logging_strategy.md](unified_logging_strategy.md): Plan to implement a centralized `IUBBLogger` with multiple sinks.
- [dependency_injection_setup.md](dependency_injection_setup.md): Plan to adopt `Microsoft.Extensions.DependencyInjection` and platform abstractions.
- [strong_typed_build_config.md](strong_typed_build_config.md): Plan to implement strong-typed argument builders for BuildGraph and UAT.

The following deployment tasks are also ongoing:

- **Final Polish & Deployment**: 
  - Verifying shell execution on Linux and macOS environments.
  - Setting up a cross-platform build pipeline via GitHub Actions.
  - Creating distribution packages (AppImage, deb, .app) for non-Windows platforms.
