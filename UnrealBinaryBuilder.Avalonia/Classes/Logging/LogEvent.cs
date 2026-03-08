using System;

namespace UnrealBinaryBuilder.Avalonia.Classes.Logging;

public enum LogLevel
{
    Debug,
    Info,
    Success,
    Warning,
    Error
}

public enum LogCategory
{
    General,
    Build,
    Git,
    Settings,
    Telemetry,
    Updater
}

public record LogEvent(string Message, LogLevel Level = LogLevel.Info, LogCategory Category = LogCategory.General, DateTime? Timestamp = null)
{
    public DateTime ActualTimestamp { get; init; } = Timestamp ?? DateTime.Now;
}
