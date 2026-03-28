using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using System;
using System.Collections.Generic;
using System.Linq;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class LocalizationService : ILocalizationService
{
    private string _currentLanguage = "en-US";

    public string CurrentLanguage => _currentLanguage;

    public void SetLanguage(string languageCode)
    {
        if (Application.Current == null) return;

        var dictionaries = Application.Current.Resources.MergedDictionaries;
        
        // Find the outer ResourceDictionary that contains our i18n includes
        var i18nDict = dictionaries.OfType<ResourceDictionary>().FirstOrDefault(d => d.MergedDictionaries.Any(m => m is ResourceInclude ri && ri.Source != null && ri.Source.ToString().Contains("/i18n/")));

        if (i18nDict != null)
        {
            var oldInclude = i18nDict.MergedDictionaries.FirstOrDefault(m => m is ResourceInclude ri && ri.Source != null && ri.Source.ToString().Contains("/i18n/"));
            if (oldInclude != null)
            {
                i18nDict.MergedDictionaries.Remove(oldInclude);
            }

            i18nDict.MergedDictionaries.Add(new ResourceInclude(new Uri($"avares://UnrealBinaryBuilder.Avalonia/Resources/i18n/{languageCode}.axaml"))
            {
                Source = new Uri($"avares://UnrealBinaryBuilder.Avalonia/Resources/i18n/{languageCode}.axaml")
            });

            _currentLanguage = languageCode;
        }
    }

    public string GetString(string key)
    {
        if (Application.Current != null && Application.Current.TryGetResource(key, null, out object? resource) && resource is string str)
        {
            return str;
        }
        return key;
    }

    public IEnumerable<LanguageInfo> GetAvailableLanguages()
    {
        yield return new LanguageInfo("en-US", "English");
        yield return new LanguageInfo("de-DE", "Deutsch");
    }
}
