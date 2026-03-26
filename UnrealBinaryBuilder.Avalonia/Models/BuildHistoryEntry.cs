using System;

namespace UnrealBinaryBuilder.Avalonia.Models;

public class BuildHistoryEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; }
    public TimeSpan Duration { get; set; }
    public bool IsSuccess { get; set; }
    public string Status { get; set; } = string.Empty;
    public string EnginePath { get; set; } = string.Empty;
    public string LogFilePath { get; set; } = string.Empty;
    public string BuildType { get; set; } = "Engine"; // "Engine" or "Plugin"
    
    // Snapshot of the settings used for this build
    public BuilderSettingsJson? SettingsSnapshot { get; set; }
}
