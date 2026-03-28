using System.Collections.Generic;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Tests;

public class MockLocalizationService : ILocalizationService
{
    public string CurrentLanguage { get; private set; } = "en-US";

    public IEnumerable<LanguageInfo> GetAvailableLanguages()
    {
        yield return new LanguageInfo("en-US", "English");
        yield return new LanguageInfo("de-DE", "Deutsch");
    }

    public string GetString(string key) => key;

    public void SetLanguage(string languageCode)
    {
        CurrentLanguage = languageCode;
    }
}
