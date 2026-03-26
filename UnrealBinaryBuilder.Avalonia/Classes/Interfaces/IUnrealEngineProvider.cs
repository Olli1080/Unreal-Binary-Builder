using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

/// <summary>
/// Provides a service for retrieving metadata and paths related to an Unreal Engine installation or source tree.
/// </summary>
public interface IUnrealEngineProvider
{
    /// <summary>
    /// Retrieves metadata for the Unreal Engine at the specified path by parsing its build configuration and version files.
    /// </summary>
    /// <param name="enginePath">The root path to the Unreal Engine.</param>
    /// <returns>The extracted <see cref="UnrealEngineMetadata"/> if successful, otherwise null.</returns>
    UnrealEngineMetadata? GetEngineMetadata(string enginePath);

    /// <summary>
    /// Gets the path to the AutomationTool script (.bat) for the given engine path, accounting for differences between UE4 and UE5.
    /// </summary>
    /// <param name="enginePath">The root path to the Unreal Engine.</param>
    /// <param name="isUE5">True if the engine is Unreal Engine 5, false if it is Unreal Engine 4.</param>
    /// <returns>The absolute path to the RunUAT.bat file.</returns>
    string GetAutomationPath(string enginePath, bool isUE5);
}
