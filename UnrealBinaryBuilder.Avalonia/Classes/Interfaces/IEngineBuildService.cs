using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

public interface IEngineBuildService
{
    /// <summary>
    /// Builds the engine based on settings and metadata.
    /// </summary>
    /// <returns>True if build succeeded, false otherwise.</returns>
    Task<bool> BuildEngineAsync(string enginePath, BuilderSettingsJson settings, VisualStudioVersion? vsVersion, UnrealEngineMetadata? metadata);

    /// <summary>
    /// Prepares the command line arguments for BuildGraph.
    /// </summary>
    string PrepareEngineCommandline(BuilderSettingsJson settings, UnrealEngineMetadata? metadata, VisualStudioVersion? vsVersion);
}
