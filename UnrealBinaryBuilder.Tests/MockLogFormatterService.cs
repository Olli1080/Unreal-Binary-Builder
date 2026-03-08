using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

namespace UnrealBinaryBuilder.Tests;

public class MockLogFormatterService : ILogFormatterService
{
    public LogFormatResult FormatLogEntryResult { get; set; } = new();

    public LogFormatResult FormatLogEntry(string message, bool isError)
    {
        return FormatLogEntryResult;
    }

    public void Reset() { }
}
