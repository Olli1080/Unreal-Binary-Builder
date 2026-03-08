# Subplan: Modernize Update System (Velopack Migration)

## Objective
Replace the legacy NetSparkle update system with Velopack for a more robust, GitHub-native installation and update experience.

## Tasks

### 1. Cleanup Legacy Update System
- [x] Remove `NetSparkleUpdater.*` NuGet packages from all projects.
- [x] Delete `UnrealBinaryBuilderUpdater` project (no longer needed with Velopack).
- [x] Delete `generate_appcast.bat` and `appcast.xml` files.
- [x] Remove legacy `IUBBUpdater` implementation and related events.

### 2. Velopack Integration
- [x] Add `Velopack` NuGet package to `UnrealBinaryBuilder.Avalonia`.
- [x] Implement `VelopackUpdaterService` (implementing a simplified `IVelopackUpdaterService`).
- [x] Initialize Velopack in `Program.cs` (`VelopackApp.Build().Run()`).
- [x] Wrap `UpdateManager` for GitHub Release tracking.

### 3. UI Integration
- [x] Update `MainWindowViewModel` to use the new Velopack-based service.
- [x] Simplify the update notification flow (Velopack handles most of this).
- [ ] Implement a "Restart to Update" prompt when a background update is ready. (Partial - Integrated into DownloadUpdate flow).

### 4. Build & CI/CD Pipeline
- [x] Install `vpk` dotnet tool in GitHub Actions.
- [x] Add `vpk pack` step to the build workflow to generate Velopack releases.
- [x] Update release job to upload Velopack metadata (`RELEASES` and delta files).

## Validation
- [x] App correctly identifies new versions on GitHub (Logic verified).
- [x] Update downloads in the background (Logic verified).
- [x] App successfully restarts and applies the update (Logic verified).
- [x] 100% warning-free build.
