namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

public enum TelemetrySeverity
{
    Info,
    Warning,
    Error,
    Critical
}

public interface ITelemetryService
{
    void Initialize(string productVersion);
    void TrackEvent(string eventName);
    void TrackProgressStart(string category, string step);
    void TrackProgressEnd(string category, string step, bool isFailure = false);
    void TrackError(string message, TelemetrySeverity severity);
    void Shutdown();
}
