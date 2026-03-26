namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

/// <summary>
/// Represents the result of a log formatting operation.
/// </summary>
public class LogFormatResult
{
    /// <summary>
    /// Gets or sets the main formatted log message.
    /// </summary>
    public string FormattedMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets text detailing compiled files, if applicable to the log entry.
    /// </summary>
    public string CompiledFilesText { get; set; } = string.Empty;
}

/// <summary>
/// Provides a service for formatting raw log messages from build tools into structured output.
/// </summary>
public interface ILogFormatterService
{
    /// <summary>
    /// Formats a raw log message into a structured <see cref="LogFormatResult"/>.
    /// </summary>
    /// <param name="message">The raw log message to format.</param>
    /// <param name="isError">Indicates whether the message should be treated as an error.</param>
    /// <returns>The formatted log result.</returns>
    LogFormatResult FormatLogEntry(string message, bool isError);

    /// <summary>
    /// Resets the internal state of the log formatter, such as compiled file counts.
    /// </summary>
    void Reset();
}
