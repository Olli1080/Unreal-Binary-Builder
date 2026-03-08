using System;

namespace UnrealBinaryBuilder.Avalonia.Classes.Logging;

public interface ILogSink
{
    void Log(LogEvent logEvent);
}

public interface IUBBLogger
{
    void Log(string message, LogLevel level = LogLevel.Info, LogCategory category = LogCategory.General);
    void Debug(string message, LogCategory category = LogCategory.General);
    void Info(string message, LogCategory category = LogCategory.General);
    void Success(string message, LogCategory category = LogCategory.General);
    void Warning(string message, LogCategory category = LogCategory.General);
    void Error(string message, LogCategory category = LogCategory.General);
    void Error(Exception exception, string? message = null, LogCategory category = LogCategory.General);
}
