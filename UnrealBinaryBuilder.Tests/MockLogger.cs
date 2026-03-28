using System;
using UnrealBinaryBuilder.Avalonia.Classes.Logging;

namespace UnrealBinaryBuilder.Tests;

public class MockLogger : IUBBLogger
{
    public void Debug(string message, LogCategory category = LogCategory.General) { }
    public void Info(string message, LogCategory category = LogCategory.General) { }
    public void Success(string message, LogCategory category = LogCategory.General) { }
    public void Warning(string message, LogCategory category = LogCategory.General) { }
    public void Error(string message, LogCategory category = LogCategory.General) { }
    public void Error(Exception exception, string? message = null, LogCategory category = LogCategory.General) { }
    public void Log(string message, LogLevel level = LogLevel.Info, LogCategory category = LogCategory.General) { }
    public void Log(LogEvent logEvent) { }
}
