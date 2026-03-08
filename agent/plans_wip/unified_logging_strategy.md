# Blueprint: Unified Logging Strategy

## Objective
Implement a single, injectable logger to handle all system and build outputs.

## Current Problems
- Fragmented logging across `MainWindowViewModel`, `BuilderSettings`, and `GameAnalyticsCSharp`.
- Logic for routing to UI and file system is duplicated or inconsistent.
- Hard to add new sinks (e.g., a real-time console).

## Target Architecture
1. **LogEvent**: A record containing message, level (Info, Warning, Error), and category (Build, Git, Settings, etc.).
2. **IUBBLogger**: The main interface for logging.
3. **AggregateLogger**: A logger that routes events to multiple `ILogSink` implementations.
4. **LogSinks**:
   - `UiLogSink`: Updates `MainWindowViewModel.LogText`.
   - `FileLogSink`: Writes to `UnrealBinaryBuilder.log`.
   - `TelemetryLogSink`: Routes errors to GameAnalytics/Sentry.

## Execution Steps

### Step 1: Definition
- Define `LogEvent` and `IUBBLogger` in `UnrealBinaryBuilder.Avalonia.Classes.Logging`.

### Step 2: Implement Sinks
- Create `ILogSink` interface.
- Implement the sinks for UI, File, and Telemetry.

### Step 3: Implement AggregateLogger
- Implement the logger that manages the collection of sinks.

### Step 4: Component Integration
- Inject `IUBBLogger` into `ProcessExecutor` and all build services.
- Replace direct `GameAnalyticsCSharp` or `File.WriteAllText` calls with the logger.

## Validation
- Ensure build errors appear in the UI, log file, and telemetry simultaneously.
- Verify log levels correctly filter/format the output.
