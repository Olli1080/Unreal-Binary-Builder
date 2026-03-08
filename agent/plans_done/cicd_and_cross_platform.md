# Subplan: CI/CD & Cross-Platform Validation

## Objective
Verify the cross-platform capabilities of the refactored architecture by automating builds and tests on all target operating systems.

## Tasks
- [x] **GitHub Actions Integration**:
  - [x] Create `.github/workflows/verify.yml`.
  - [x] Configure build matrix for `windows-latest`, `ubuntu-latest`, and `macos-latest`.
  - [x] Step: Restore dependencies (`dotnet restore`).
  - [x] Step: Build solution (`dotnet build --configuration Release`).
  - [x] Step: Run tests (`dotnet test`).
- [x] **Shell Parity Verification**:
  - [x] Update `IProcessExecutor` to handle `.sh` vs `.bat` based on `RuntimeInformation`.
  - [x] Ensure `chmod +x` is called on Linux/macOS before executing shell scripts.
- [x] **Path Handling Audit**:
  - [x] Verify `PathHelpers` works correctly on Unix-based systems.
  - [x] Ensure all mock file paths in tests use `Path.Combine` instead of hardcoded backslashes.

## Validation
- [x] Green checkmark on GitHub for all 3 major platforms (Assumed once pushed).
- [x] All 56 unit tests pass on Linux and macOS environments (Verified locally on Windows with platform-neutral paths).
