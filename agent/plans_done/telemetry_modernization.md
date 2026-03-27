# Telemetry & Analytics Modernization

This plan focuses on decoupling the application from the specific telemetry provider (GameAnalytics) by introducing a service-oriented approach.

## Goals
- Decouple business logic from `GameAnalyticsSDK`.
- Improve testability by allowing telemetry to be mocked.
- Centralize telemetry event names to avoid magic strings.

## Tasks
- [x] Create `ITelemetryService` interface.
- [x] Implement `GameAnalyticsTelemetryService` implementing `ITelemetryService`.
- [x] Create `TelemetryConstants` for event names and categories.
- [x] Register `ITelemetryService` in `App.axaml.cs` dependency injection.
- [x] Update `EngineBuildService`, `PluginBuildService`, and `MainWindowViewModel` to use `ITelemetryService`.
- [x] Remove direct dependencies on the static `GameAnalyticsCSharp` class.
- [x] Add unit tests for telemetry service integration.
