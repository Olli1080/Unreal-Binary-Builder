using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Classes.Logging;
using UnrealBinaryBuilder.Avalonia.ViewModels;

namespace UnrealBinaryBuilder.Avalonia.Classes;

/// <summary>
/// Provides services for building Unreal Engine plugins.
/// </summary>
public class PluginBuildService : IPluginBuildService
{
    private readonly IProcessExecutor _processExecutor;
    private readonly IUBBLogger _logger;
    private readonly IZipService _zipService;
    private readonly ITelemetryService _telemetry;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginBuildService"/> class.
    /// </summary>
    /// <param name="processExecutor">The process executor.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="zipService">The zip service.</param>
    /// <param name="telemetry">The telemetry service.</param>
    public PluginBuildService(IProcessExecutor processExecutor, IUBBLogger logger, IZipService zipService, ITelemetryService telemetry)
    {
        _processExecutor = processExecutor;
        _logger = logger;
        _zipService = zipService;
        _telemetry = telemetry;
    }

    /// <summary>
    /// Asynchronously builds a queue of plugins.
    /// </summary>
    /// <param name="pluginQueue">A collection of view models representing the plugins to build.</param>
    /// <returns>A task that represents the asynchronous build operation. The task result contains a boolean indicating whether all plugins were built successfully.</returns>
    public async Task<bool> BuildPluginsAsync(IEnumerable<PluginCardViewModel> pluginQueue)
    {
        var plugins = pluginQueue.ToList();
        if (plugins.Count == 0) return true;

        foreach (var plugin in plugins)
        {
            _telemetry.TrackProgressStart(TelemetryConstants.CAT_BUILD, TelemetryConstants.STEP_PLUGIN);
            plugin.StartBuild();
            
            string args = BuildArgumentBuilder.BuildPluginArguments(plugin).ToString();
            int ec = await _processExecutor.ExecuteAsync(plugin.RunUATFile, args, Path.GetDirectoryName(plugin.RunUATFile)!, LogCategory.Build);
            bool success = ec == 0;
            
            if (success && plugin.CanZip)
            {
                _telemetry.TrackEvent($"{TelemetryConstants.EVENT_ZIP_STARTED}:{plugin.PluginName}");
                try
                {
                    await _zipService.SavePluginToZip(plugin.PluginPath, plugin.TargetZipPath, plugin.ZipForMarketplaceZip, true);
                    _telemetry.TrackEvent($"{TelemetryConstants.EVENT_ZIP_FINISHED}:{plugin.PluginName}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to zip plugin: {plugin.PluginName}", LogCategory.Build);
                }
            }

            plugin.FinishBuild(success);
            _telemetry.TrackProgressEnd(TelemetryConstants.CAT_BUILD, TelemetryConstants.STEP_PLUGIN, !success);

            if (!success)
            {
                _logger.Error($"Plugin Build Failed: {plugin.PluginName}", LogCategory.Build);
                return false;
            }
        }

        return true;
    }
}
