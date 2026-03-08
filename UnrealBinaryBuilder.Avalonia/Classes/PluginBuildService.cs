using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Classes.Logging;
using UnrealBinaryBuilder.Avalonia.ViewModels;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class PluginBuildService : IPluginBuildService
{
    private readonly IProcessExecutor _processExecutor;
    private readonly IUBBLogger _logger;
    private readonly IZipService _zipService;

    public PluginBuildService(IProcessExecutor processExecutor, IUBBLogger logger, IZipService zipService)
    {
        _processExecutor = processExecutor;
        _logger = logger;
        _zipService = zipService;
    }

    public async Task<bool> BuildPluginsAsync(IEnumerable<PluginCardViewModel> pluginQueue)
    {
        var plugins = pluginQueue.ToList();
        if (plugins.Count == 0) return true;

        foreach (var plugin in plugins)
        {
            GameAnalyticsCSharp.AddProgressStart("Build", "Plugin");
            plugin.StartBuild();
            
            string args = BuildArgumentBuilder.BuildPluginArguments(plugin).ToString();
            int ec = await _processExecutor.ExecuteAsync(plugin.RunUATFile, args, Path.GetDirectoryName(plugin.RunUATFile)!, LogCategory.Build);
            bool success = ec == 0;
            
            if (success && plugin.bCanZip)
            {
                GameAnalyticsCSharp.AddDesignEvent($"ZipPlugin:Started:{plugin.PluginName}");
                try
                {
                    await _zipService.SavePluginToZip(plugin.PluginPath, plugin.TargetZipPath, plugin.bZipForMarketplaceZip, true);
                    GameAnalyticsCSharp.AddDesignEvent($"ZipPlugin:Finished:{plugin.PluginName}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to zip plugin: {plugin.PluginName}", LogCategory.Build);
                }
            }

            plugin.FinishBuild(success);
            GameAnalyticsCSharp.AddProgressEnd("Build", "Plugin", !success);

            if (!success)
            {
                _logger.Error($"Plugin Build Failed: {plugin.PluginName}", LogCategory.Build);
                return false;
            }
        }

        return true;
    }
}
