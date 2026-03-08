using System.Collections.Generic;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.ViewModels;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

public interface IPluginBuildService
{
    /// <summary>
    /// Orchestrates the build of a queue of plugins.
    /// </summary>
    /// <returns>True if all plugins built successfully, false otherwise.</returns>
    Task<bool> BuildPluginsAsync(IEnumerable<PluginCardViewModel> pluginQueue);
}
