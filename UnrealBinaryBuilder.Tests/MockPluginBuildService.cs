using System.Collections.Generic;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.ViewModels;

namespace UnrealBinaryBuilder.Tests;

public class MockPluginBuildService : IPluginBuildService
{
    public bool BuildPluginsAsyncResult { get; set; } = true;

    public Task<bool> BuildPluginsAsync(IEnumerable<PluginCardViewModel> pluginQueue)
    {
        return Task.FromResult(BuildPluginsAsyncResult);
    }
}
