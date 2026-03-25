using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

namespace UnrealBinaryBuilder.Avalonia.Classes.Logging;

public class TelemetryLogSink : ILogSink
{
    private readonly ITelemetryService _telemetry;

    public TelemetryLogSink(ITelemetryService telemetry)
    {
        _telemetry = telemetry;
    }

    public void Log(LogEvent logEvent)
    {
        if (logEvent.Level == LogLevel.Error)
        {
            _telemetry.TrackError(logEvent.Message, TelemetrySeverity.Error);
        }
        else if (logEvent.Level == LogLevel.Warning)
        {
            _telemetry.TrackError(logEvent.Message, TelemetrySeverity.Warning);
        }
    }
}
