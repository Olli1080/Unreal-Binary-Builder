using System;

namespace UnrealBinaryBuilder.Avalonia.Classes.Logging;

public class UiLogSink : ILogSink
{
    public event Action<LogEvent>? OnLog;

    public void Log(LogEvent logEvent)
    {
        OnLog?.Invoke(logEvent);
    }
}
