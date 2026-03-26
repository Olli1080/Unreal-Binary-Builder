using System.Collections.Generic;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.ViewModels;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

/// <summary>
/// Provides a service for orchestrating the building of Unreal Engine plugins.
/// </summary>
public interface IPluginBuildService
{
    /// <summary>
    /// Orchestrates the build of a queue of plugins asynchronously.
    /// </summary>
    /// <param name="pluginQueue">A collection of view models representing the plugins to build.</param>
    /// <returns>A task that represents the asynchronous build operation. The task result is true if all plugins built successfully, false otherwise.</returns>
    Task<bool> BuildPluginsAsync(IEnumerable<PluginCardViewModel> pluginQueue);
}
