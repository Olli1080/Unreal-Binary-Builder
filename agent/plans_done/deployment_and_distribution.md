# Subplan: Deployment & Distribution

## Objective
Automate the creation of production-ready distribution packages for Windows, Linux, and macOS.

## Tasks
- [x] **Portable Packaging**:
  - [x] Configure `dotnet publish` with `PublishSingleFile=true` for all platforms.
  - [x] Create automated `.zip` archiving for each build variant.
- [x] **CI/CD Integration**:
  - [x] Update GitHub Actions (`build-ubb.yml`) to trigger on version tags (`v*`).
  - [x] Create manual Publish workflow (`publish.yml`) with version input and Velopack integration.
  - [x] Implement automated artifact upload to GitHub Releases using `softprops/action-gh-release`.
- [x] **Multi-Platform Support**:
  - [x] Automated builds for `win-x64`, `linux-x64`, `osx-x64`, and `osx-arm64`.

## Validation
- [x] Build artifacts are correctly zipped and uploaded to GitHub.
- [x] Verified 100% test pass rate (56/56).
