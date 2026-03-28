# Internationalization (i18n)

This plan focused on adding multi-language support to Unreal Binary Builder using dynamic resource loading.

## Goals
- Support runtime language switching without application restart.
- Centralize all user-facing strings into resource dictionaries.
- Provide initial support for English and German.

## Tasks
- [x] Create `ILocalizationService` and `LocalizationService`.
- [x] Implement dynamic `ResourceDictionary` swapping logic.
- [x] Create initial language files (`en-US.axaml`, `de-DE.axaml`).
- [x] Integrate localization into `MainWindowViewModel` and `AppInitializer`.
- [x] Update XAML views to use `{DynamicResource}` for all localized strings.
- [x] Add a language selector to the Settings menu.
- [x] Ensure persistence of user language preference.
