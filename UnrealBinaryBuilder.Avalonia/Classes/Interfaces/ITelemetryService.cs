namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

/// <summary>
/// Specifies the severity level of a telemetry error event.
/// </summary>
public enum TelemetrySeverity
{
    /// <summary>Informational message.</summary>
    Info,
    /// <summary>Warning message indicating a potential issue.</summary>
    Warning,
    /// <summary>Error message indicating a failure.</summary>
    Error,
    /// <summary>Critical error message indicating a severe failure.</summary>
    Critical
}

/// <summary>
/// Provides a service for tracking application usage, progress, and errors via telemetry.
/// </summary>
public interface ITelemetryService
{
    /// <summary>
    /// Initializes the telemetry service with the application's product version.
    /// </summary>
    /// <param name="productVersion">The version string of the application.</param>
    void Initialize(string productVersion);

    /// <summary>
    /// Tracks a specific named event.
    /// </summary>
    /// <param name="eventName">The name of the event to track.</param>
    void TrackEvent(string eventName);

    /// <summary>
    /// Tracks the start of a progress step within a category.
    /// </summary>
    /// <param name="category">The category of the progress (e.g., 'Build').</param>
    /// <param name="step">The specific step starting (e.g., 'Setup').</param>
    void TrackProgressStart(string category, string step);

    /// <summary>
    /// Tracks the end of a progress step within a category, optionally marking it as a failure.
    /// </summary>
    /// <param name="category">The category of the progress.</param>
    /// <param name="step">The specific step ending.</param>
    /// <param name="isFailure">True if the step failed, false if it succeeded.</param>
    void TrackProgressEnd(string category, string step, bool isFailure = false);

    /// <summary>
    /// Tracks an error message with a specified severity level.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="severity">The severity of the error.</param>
    void TrackError(string message, TelemetrySeverity severity);

    /// <summary>
    /// Shuts down the telemetry service, ensuring all pending events are flushed.
    /// </summary>
    void Shutdown();
}
