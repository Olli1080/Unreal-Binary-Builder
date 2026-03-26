# Consistency & Standards

This plan focuses on aligning the codebase with modern C# standards and improving developer experience through documentation.

## Goals
- Standardize naming conventions across models and services.
- Improve code discoverability with comprehensive XML documentation.
- Ensure consistent use of null-safety features.

## Tasks
- [x] **Naming Standardization**:
    - Rename properties in `BuilderSettingsJson.cs` to remove Unreal-style `b` prefixes (e.g., `WithWin64` instead of `bWithWin64`).
    - Update all XAML bindings and references to reflect naming changes.
    - Ensure JSON persistence remains compatible (using `[JsonPropertyName]` if needed).
- [x] **XML Documentation**:
    - Add `///` comments to all public interfaces in `Classes/Interfaces/`.
    - Document core methods in `EngineBuildService`, `PluginBuildService`, and `GitService`.
- [ ] **Code Cleanup**:
    - Remove any remaining unused using statements or dead code.
    - Ensure consistent use of file-scoped namespaces.
