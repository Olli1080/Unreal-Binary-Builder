# Error Handling & Resilience

This plan focuses on making the application more robust against unexpected failures and providing better feedback to the user.

## Goals
- Implement a global "safety net" for unhandled exceptions.
- Standardize how errors are reported to the user across different services.
- Improve logging for diagnostic purposes.

## Tasks
- [x] **Global Exception Handling**:
    - Implement a centralized handler in `App.axaml.cs` for `TaskScheduler.UnobservedTaskException` and `AppDomain.CurrentDomain.UnhandledException`.
    - Show a user-friendly "Unexpected Error" dialog via `IUIService`.
- [x] **Service-Level Resilience**:
    - Audit all `async` methods for proper `try-catch` blocks.
    - Ensure the UI remains responsive and informative even when a background task fails.
- [x] **Enhanced Logging**:
    - Include more context (system info, stack traces) in error logs.
