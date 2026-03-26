using System;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

/// <summary>
/// Provides a service for archiving build outputs and plugins into ZIP files.
/// </summary>
public interface IZipService
{
    /// <summary>
    /// Determines whether the application can safely save to the specified ZIP file path (e.g., checking if it's already in use).
    /// </summary>
    /// <param name="zipPath">The path to the destination ZIP file.</param>
    /// <returns>True if the path is available and writable, false otherwise.</returns>
    bool CanSaveToZip(string zipPath);

    /// <summary>
    /// Prepares the service for a new ZIP operation, resetting any cancellation tokens or states.
    /// </summary>
    void PrepareToSave();

    /// <summary>
    /// Cancels the currently running ZIP compression task.
    /// </summary>
    void CancelTask();

    /// <summary>
    /// Asynchronously compresses a plugin directory into a ZIP file.
    /// </summary>
    /// <param name="sourcePath">The source directory path of the plugin to compress.</param>
    /// <param name="zipLocationToSave">The destination path for the resulting ZIP file.</param>
    /// <param name="zipForMarketplace">True if the ZIP should be structured for Unreal Engine Marketplace submission.</param>
    /// <param name="fastCompression">True to use faster but less efficient compression.</param>
    /// <param name="progress">An optional progress provider to report compression progress.</param>
    /// <returns>A task that represents the asynchronous compression operation.</returns>
    Task SavePluginToZip(string sourcePath, string zipLocationToSave, bool zipForMarketplace, bool fastCompression, IProgress<ZipProgress>? progress = null);

    /// <summary>
    /// Asynchronously compresses a completed engine build directory into a ZIP file based on builder settings.
    /// </summary>
    /// <param name="inBuildDirectory">The directory containing the compiled engine build.</param>
    /// <param name="zipLocationToSave">The destination path for the resulting ZIP file.</param>
    /// <param name="settings">The builder settings configuring which components to include in the ZIP.</param>
    /// <param name="progress">An optional progress provider to report compression progress.</param>
    /// <returns>A task that represents the asynchronous compression operation.</returns>
    Task SaveToZip(string inBuildDirectory, string zipLocationToSave, BuilderSettingsJson settings, IProgress<ZipProgress>? progress = null);
}
