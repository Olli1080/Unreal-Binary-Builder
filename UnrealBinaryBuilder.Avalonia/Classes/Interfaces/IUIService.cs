using System;
using System.Threading.Tasks;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

/// <summary>
/// Specifies the type of notification to display.
/// </summary>
public enum UBBNotificationType 
{ 
    /// <summary>An informational notification.</summary>
    Info, 
    /// <summary>A success notification.</summary>
    Success, 
    /// <summary>A warning notification.</summary>
    Warning, 
    /// <summary>An error notification.</summary>
    Error 
}

/// <summary>
/// Represents the arguments for a notification event.
/// </summary>
/// <param name="Title">The title of the notification.</param>
/// <param name="Message">The main content message of the notification.</param>
/// <param name="Type">The type of the notification.</param>
public record NotificationEventArgs(string Title, string Message, UBBNotificationType Type);

/// <summary>
/// Specifies the result of a user interaction with a dialog.
/// </summary>
public enum UBBDialogResult 
{ 
    /// <summary>No selection was made or the dialog was dismissed.</summary>
    None, 
    /// <summary>The primary action button was selected.</summary>
    Primary, 
    /// <summary>The secondary action button was selected.</summary>
    Secondary 
}

/// <summary>
/// Provides a service for interacting with the application's User Interface, displaying dialogs, notifications, and opening files/folders.
/// </summary>
public interface IUIService
{
    /// <summary>
    /// Event triggered when a notification needs to be shown to the user.
    /// </summary>
    event EventHandler<NotificationEventArgs>? ShowNotification;

    /// <summary>
    /// Shows a transient toast notification on the screen.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="type">The type of toast notification.</param>
    /// <param name="title">The optional title of the toast.</param>
    void ShowToast(string message, UBBNotificationType type = UBBNotificationType.Info, string title = "");

    /// <summary>
    /// Shows a modal message dialog asynchronously and waits for the user's response.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="content">The main content message of the dialog.</param>
    /// <param name="primaryButton">The text for the primary action button.</param>
    /// <param name="secondaryButton">The text for an optional secondary action button.</param>
    /// <param name="closeButton">The text for an optional close button.</param>
    /// <returns>A task representing the user's selection result from the dialog.</returns>
    Task<UBBDialogResult> ShowMessageDialog(string title, string content, string primaryButton = "OK", string? secondaryButton = null, string? closeButton = null);

    /// <summary>
    /// Applies a specific theme to the application UI.
    /// </summary>
    /// <param name="theme">The name of the theme to apply.</param>
    void ApplyTheme(string theme);

    /// <summary>
    /// Opens the specified URL in the system's default web browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    void OpenUrl(string url);

    /// <summary>
    /// Opens the specified folder path in the system's default file explorer.
    /// </summary>
    /// <param name="path">The folder path to open.</param>
    void OpenFolder(string path);

    /// <summary>
    /// Shows the "About" dialog for the application.
    /// </summary>
    void ShowAboutDialog();

    /// <summary>
    /// Opens the specified file path in the system's default code or text editor.
    /// </summary>
    /// <param name="filePath">The path of the file to edit.</param>
    void OpenCodeEditor(string filePath);

    /// <summary>
    /// Copies the specified text to the system clipboard asynchronously.
    /// </summary>
    /// <param name="text">The text to copy.</param>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    Task CopyTextToClipboard(string text);

    /// <summary>
    /// Opens a folder browser dialog asynchronously to allow the user to select a directory.
    /// </summary>
    /// <param name="title">The title of the browser dialog.</param>
    /// <returns>A task whose result is the selected folder path, or null if cancelled.</returns>
    Task<string?> BrowseFolderAsync(string title);

    /// <summary>
    /// Opens a file browser dialog asynchronously to allow the user to select a file.
    /// </summary>
    /// <param name="title">The title of the browser dialog.</param>
    /// <param name="extensions">An array of allowed file extensions.</param>
    /// <param name="filterName">The name of the filter for the extensions.</param>
    /// <returns>A task whose result is the selected file path, or null if cancelled.</returns>
    Task<string?> BrowseFileAsync(string title, string[] extensions, string filterName);

    /// <summary>
    /// Opens a save file dialog asynchronously to allow the user to specify a location and name to save a file.
    /// </summary>
    /// <param name="title">The title of the save dialog.</param>
    /// <param name="suggestedName">The default suggested file name.</param>
    /// <param name="extension">The default file extension.</param>
    /// <param name="filterName">The name of the filter for the extension.</param>
    /// <returns>A task whose result is the selected save path, or null if cancelled.</returns>
    Task<string?> SaveFileAsync(string title, string suggestedName, string extension, string filterName);
}
