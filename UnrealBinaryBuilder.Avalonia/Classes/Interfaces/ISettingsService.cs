using UnrealBinaryBuilder.Avalonia.Models;
using System;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

public interface ISettingsService
{
    BuilderSettingsJson GetSettings();
    void SaveSettings(BuilderSettingsJson settings);
    void WriteToLogFile(string content);
    void WriteErrorsToLogFile(string content);
    void OpenLogFolder();
    void OpenSettings();
    
    event Action<string, LogMessageType>? OnLog;
}
