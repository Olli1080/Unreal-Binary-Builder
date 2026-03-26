using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

namespace UnrealBinaryBuilder.Avalonia.Classes.Logging;

public class FileLogSink : ILogSink
{
    private readonly ISettingsService _settingsService;

    public FileLogSink(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public void Log(LogEvent logEvent)
    {
        var timestamp = logEvent.ActualTimestamp.ToString("yyyy-MM-dd HH:mm:ss");
        var formattedMessage = $"[{timestamp}] [{logEvent.Level}] [{logEvent.Category}] {logEvent.Message}";
        
        _settingsService.WriteToLogFile(formattedMessage);

        if (logEvent.Level == LogLevel.Error)
        {
            _settingsService.WriteErrorsToLogFile(formattedMessage);
        }
    }
}
