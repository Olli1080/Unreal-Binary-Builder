using GameAnalyticsSDK.Net;

namespace UnrealBinaryBuilder.Avalonia.Classes.Logging;

public class TelemetryLogSink : ILogSink
{
    public void Log(LogEvent logEvent)
    {
        if (logEvent.Level == LogLevel.Error)
        {
            GameAnalyticsCSharp.LogEvent(logEvent.Message, EGAErrorSeverity.Error);
        }
        else if (logEvent.Level == LogLevel.Warning)
        {
            GameAnalyticsCSharp.LogEvent(logEvent.Message, EGAErrorSeverity.Warning);
        }
    }
}
