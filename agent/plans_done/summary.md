# Plans Done

The following major plans and milestones have been completed:

- **Initial Setup**: Core functionality for building "Installed Builds" of Unreal Engine.
- **Unreal Engine 5 Support**: Added compatibility for building UE5 from source.
- **.NET 9.0 Migration**: Main application migrated to .NET 9.0 for improved performance and features.
- **Observability**: Integrated Sentry for crash reporting and GameAnalytics for usage tracking.
- **Agent Documentation Infrastructure**: Set up `GEMINI.md`, `AGENTS.md`, and the `agent/` directory structure.
- **UI Framework Evaluation**: Evaluated various frameworks and selected Avalonia UI as the state-of-the-art cross-platform target.
- **Avalonia UI Port (Phase 3 Complete)**: 
  - Successfully migrated the entire WPF application to Avalonia UI on **.NET 10.0**.
  - Reached **100% feature parity** including:
    - Full engine and plugin build orchestration.
    - Chained setup automation (`Setup.bat` -> `GenerateProjectFiles` -> `AutomationTool`).
    - Visual Studio selection and Git metadata integration.
    - Cross-platform file/folder picking and shell execution.
    - Real-time build timing and compilation progress tracking.
    - Integrated Toast Notifications and Sentry v5/v6 crash reporting.
    - Theme switching (Light/Dark/System) and Window state persistence.
    - Modern `NavigationView` based layout with full navigation parity.
- **Continuous Integration**:
  - Implemented cross-platform GitHub Actions build matrix for Windows, Linux, and macOS.
