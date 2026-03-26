namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

/// <summary>
/// Provides a platform-agnostic service for interacting with the operating system.
/// </summary>
public interface IPlatformService
{
    /// <summary>
    /// Opens the specified folder path in the system's default file explorer.
    /// </summary>
    /// <param name="path">The directory path to open.</param>
    void OpenFolder(string path);

    /// <summary>
    /// Opens the specified URL in the system's default web browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    void OpenUrl(string url);

    /// <summary>
    /// Initiates a shutdown of the computer after the specified number of seconds.
    /// </summary>
    /// <param name="seconds">The delay in seconds before shutting down.</param>
    void ShutdownPC(int seconds);
}
