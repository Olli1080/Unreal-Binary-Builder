using System;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

public class LogFormatResult
{
    public string FormattedMessage { get; set; } = string.Empty;
    public string CompiledFilesText { get; set; } = string.Empty;
}

public interface ILogFormatterService
{
    LogFormatResult FormatLogEntry(string message, bool isError);
    void Reset();
}
