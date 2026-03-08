# Subplan: UI & Dialog Service Extraction

## Objective
Extract UI-related logic (notifications, dialogs, navigation) from `MainWindowViewModel` into a dedicated `IUIService`.

## Tasks
- [x] Define `IUIService` in `UnrealBinaryBuilder.Avalonia.Classes.Interfaces`.
  - Methods: `ShowToast`, `ShowMessageDialog`, `ApplyTheme`, `OpenUrl`, `OpenFolder`.
- [x] Implement `UIService` in `UnrealBinaryBuilder.Avalonia.Classes`.
  - Use `Application.Current` and `IClassicDesktopStyleApplicationLifetime` for dialogs.
- [x] Move theme application logic from ViewModel to `UIService`.
- [x] Update `MainWindowViewModel` to use `IUIService` for notifications and dialogs.

## Validation
- [x] Verify toasts still appear correctly (via Unit Test).
- [x] Verify message dialogs function as expected.
- [x] Verify external links (Support, Changelog, etc.) still work.
