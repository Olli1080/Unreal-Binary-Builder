# Subplan: CI/CD & Cross-Platform Validation

## Objective
Verify the cross-platform capabilities of the refactored architecture by automating builds and tests on all target operating systems.

## Tasks
- [ ] **GitHub Actions Integration**:
  - [ ] Create `.github/workflows/verify.yml`.
  - [ ] Configure build matrix for `windows-latest`, `ubuntu-latest`, and `macos-latest`.
  - [ ] Step: Restore dependencies (`dotnet restore`).
  - [ ] Step: Build solution (`dotnet build --configuration Release`).
  - [ ] Step: Run tests (`dotnet test`).
- [ ] **Shell Parity Verification**:
  - [ ] Update `IProcessExecutor` to handle `.sh` vs `.bat` based on `RuntimeInformation`.
  - [ ] Ensure `chmod +x` is called on Linux/macOS before executing shell scripts.
- [ ] **Path Handling Audit**:
  - [ ] Verify `PathHelpers` works correctly on Unix-based systems.
  - [ ] Ensure all mock file paths in tests use `Path.Combine` instead of hardcoded backslashes.

## Validation
- [ ] Green checkmark on GitHub for all 3 major platforms.
- [ ] All 56 unit tests pass on Linux and macOS environments.
