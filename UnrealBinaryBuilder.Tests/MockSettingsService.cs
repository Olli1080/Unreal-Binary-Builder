using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;
using System;

namespace UnrealBinaryBuilder.Tests;

public class MockSettingsService : ISettingsService
{
    private BuilderSettingsJson _settings = new();

    public event Action<string, LogMessageType>? OnLog;

    public BuilderSettingsJson GetSettings() => _settings;

    public void SaveSettings(BuilderSettingsJson settings) { _settings = settings; }

    public void OpenLogFolder() { }

    public void OpenSettings() { }

    public void WriteToLogFile(string content) { }

    public void WriteErrorsToLogFile(string content) { }
}
