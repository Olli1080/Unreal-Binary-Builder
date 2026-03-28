using System.Collections.Generic;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

/// <summary>
/// Service responsible for managing application localization and language switching.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the current language code (e.g., "en-US").
    /// </summary>
    string CurrentLanguage { get; }

    /// <summary>
    /// Switches the application language at runtime.
    /// </summary>
    /// <param name="languageCode">The culture code to switch to (e.g., "en-US", "de-DE").</param>
    void SetLanguage(string languageCode);

    /// <summary>
    /// Gets a localized string by its key.
    /// </summary>
    string GetString(string key);

    /// <summary>
    /// Gets a list of available languages and their display names.
    /// </summary>
    IEnumerable<LanguageInfo> GetAvailableLanguages();
}
