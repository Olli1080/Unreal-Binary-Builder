using System;
using System.Collections.Generic;

namespace UnrealBinaryBuilder.Avalonia.Classes.Logging;

public class AggregateLogger : IUBBLogger
{
    private readonly IEnumerable<ILogSink> _sinks;

    public AggregateLogger(IEnumerable<ILogSink> sinks)
    {
        _sinks = sinks;
    }

    public void Log(LogEvent logEvent)
    {
        foreach (var sink in _sinks)
        {
            try
            {
                sink.Log(logEvent);
            }
            catch
            {
                // Silently ignore sink failures to avoid crashing during logging
            }
        }
    }

    public void Log(string message, LogLevel level = LogLevel.Info, LogCategory category = LogCategory.General)
        => Log(new LogEvent(message, level, category));

    public void Debug(string message, LogCategory category = LogCategory.General)
        => Log(message, LogLevel.Debug, category);

    public void Info(string message, LogCategory category = LogCategory.General)
        => Log(message, LogLevel.Info, category);

    public void Success(string message, LogCategory category = LogCategory.General)
        => Log(message, LogLevel.Success, category);

    public void Warning(string message, LogCategory category = LogCategory.General)
        => Log(message, LogLevel.Warning, category);

    public void Error(string message, LogCategory category = LogCategory.General)
        => Log(message, LogLevel.Error, category);

    public void Error(Exception exception, string? message = null, LogCategory category = LogCategory.General)
    {
        var msg = string.IsNullOrEmpty(message) ? exception.Message : $"{message}: {exception.Message}";
        Log(msg, LogLevel.Error, category);
        // We could also log the stack trace if needed
        Log(exception.StackTrace ?? "No stack trace", LogLevel.Debug, category);
    }
}
