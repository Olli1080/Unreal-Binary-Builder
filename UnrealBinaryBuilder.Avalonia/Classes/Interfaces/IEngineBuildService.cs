using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

/// <summary>
/// Provides services for building Unreal Engine from source using BuildGraph.
/// </summary>
public interface IEngineBuildService
{
    /// <summary>
    /// Builds the Unreal Engine based on the provided settings, Visual Studio version, and metadata.
    /// </summary>
    /// <param name="enginePath">The root path to the Unreal Engine source code.</param>
    /// <param name="settings">The builder settings configuring the build.</param>
    /// <param name="vsVersion">The specific Visual Studio version to use for the build.</param>
    /// <param name="metadata">The metadata of the Unreal Engine being built.</param>
    /// <returns>True if the engine build succeeded, otherwise false.</returns>
    Task<bool> BuildEngineAsync(string enginePath, BuilderSettingsJson settings, VisualStudioVersion? vsVersion, UnrealEngineMetadata? metadata);

    /// <summary>
    /// Prepares the command line arguments necessary for executing the BuildGraph script to build the engine.
    /// </summary>
    /// <param name="settings">The builder settings configuring the build.</param>
    /// <param name="metadata">The metadata of the Unreal Engine being built.</param>
    /// <param name="vsVersion">The specific Visual Studio version to use for the build.</param>
    /// <returns>A formatted string of command line arguments for BuildGraph.</returns>
    string PrepareEngineCommandline(BuilderSettingsJson settings, UnrealEngineMetadata? metadata, VisualStudioVersion? vsVersion);
}
