using Microsoft.Extensions.DependencyInjection;
using System;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

namespace UnrealBinaryBuilder.Avalonia.Classes.Logging;

public class TelemetryLogSink : ILogSink
{
    private readonly IServiceProvider _serviceProvider;
    private ITelemetryService? _telemetry;

    public TelemetryLogSink(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Log(LogEvent logEvent)
    {
        _telemetry ??= _serviceProvider.GetService<ITelemetryService>();
        if (_telemetry == null) return;

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
