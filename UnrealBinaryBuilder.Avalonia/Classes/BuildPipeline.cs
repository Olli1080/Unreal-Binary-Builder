using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Classes.Logging;
using UnrealBinaryBuilder.Avalonia.Models;
using UnrealBinaryBuilder.Avalonia.ViewModels;

namespace UnrealBinaryBuilder.Avalonia.Classes;

/// <summary>
/// Orchestrates the sequence of build operations for engine and plugins.
/// </summary>
public class BuildPipeline : IBuildPipeline
{
    private readonly ISetupService _setupService;
    private readonly IEngineBuildService _engineBuildService;
    private readonly IPluginBuildService _pluginBuildService;
    private readonly IZipService _zipService;
    private readonly IPlatformService _platformService;
    private readonly IUBBLogger _logger;
    private readonly ITelemetryService _telemetry;

    public BuildPipeline(
        ISetupService setupService,
        IEngineBuildService engineBuildService,
        IPluginBuildService pluginBuildService,
        IZipService zipService,
        IPlatformService platformService,
        IUBBLogger logger,
        ITelemetryService telemetry)
    {
        _setupService = setupService;
        _engineBuildService = engineBuildService;
        _pluginBuildService = pluginBuildService;
        _zipService = zipService;
        _platformService = platformService;
        _logger = logger;
        _telemetry = telemetry;
    }

    public async Task<bool> ExecuteEnginePipelineAsync(
        string enginePath, 
        BuilderSettingsJson settings, 
        VisualStudioMsBuild? msBuild, 
        string architecture, 
        VisualStudioVersion? vsVersion, 
        UnrealEngineMetadata? metadata, 
        IProgress<BuildPipelineProgress>? progress = null)
    {
        _logger.Info("Starting Engine Build Pipeline...", LogCategory.Build);

        // 1. Setup Phase
        if (settings.BuildSetupBatFile || settings.GenerateProjectFiles || settings.BuildAutomationTool)
        {
            progress?.Report(new BuildPipelineProgress { Stage = BuildPipelineStage.Setup, Message = "Running Setup...", OverallPercentage = 10 });
            bool setupSuccess = await _setupService.RunSetupChainAsync(enginePath, settings, msBuild, architecture);
            if (!setupSuccess)
            {
                progress?.Report(new BuildPipelineProgress { Stage = BuildPipelineStage.Failed, Message = "Setup failed." });
                return false;
            }
        }

        // 2. Build Phase
        progress?.Report(new BuildPipelineProgress { Stage = BuildPipelineStage.EngineBuild, Message = "Building Engine...", OverallPercentage = 40 });
        bool buildSuccess = await _engineBuildService.BuildEngineAsync(enginePath, settings, vsVersion, metadata);
        if (!buildSuccess)
        {
            progress?.Report(new BuildPipelineProgress { Stage = BuildPipelineStage.Failed, Message = "Engine build failed." });
            return false;
        }

        // 3. Zip Phase
        if (settings.ZipEngineBuild && !string.IsNullOrEmpty(settings.ZipEnginePath))
        {
            progress?.Report(new BuildPipelineProgress { Stage = BuildPipelineStage.Zipping, Message = "Zipping Engine...", OverallPercentage = 80 });
            _telemetry.TrackEvent(TelemetryConstants.EVENT_ZIP_STARTED);
            try
            {
                await _zipService.SaveToZip(Path.Combine(enginePath, "LocalBuilds", "Engine"), settings.ZipEnginePath, settings);
                _telemetry.TrackEvent(TelemetryConstants.EVENT_ZIP_FINISHED);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to zip engine build.", LogCategory.Build);
                // We don't necessarily want to fail the whole pipeline if just zipping fails, but we should report it.
            }
        }

        // 4. Post-Build Actions
        if (settings.ShutdownIfBuildSuccess && settings.ShutdownPC)
        {
            _logger.Info("Shutting down PC in 5 seconds...", LogCategory.General); 
            _telemetry.TrackEvent(TelemetryConstants.EVENT_SHUTDOWN_STARTED); 
            _platformService.ShutdownPC(5);
            Environment.Exit(0);
        }

        progress?.Report(new BuildPipelineProgress { Stage = BuildPipelineStage.Finished, Message = "Pipeline finished successfully.", OverallPercentage = 100 });
        return true;
    }

    public async Task<bool> ExecutePluginPipelineAsync(IEnumerable<PluginCardViewModel> pluginQueue, IProgress<BuildPipelineProgress>? progress = null)
    {
        progress?.Report(new BuildPipelineProgress { Stage = BuildPipelineStage.PluginBuild, Message = "Building Plugins...", OverallPercentage = 0 });
        bool success = await _pluginBuildService.BuildPluginsAsync(pluginQueue);
        
        if (success)
        {
            progress?.Report(new BuildPipelineProgress { Stage = BuildPipelineStage.Finished, Message = "Plugin builds finished.", OverallPercentage = 100 });
        }
        else
        {
            progress?.Report(new BuildPipelineProgress { Stage = BuildPipelineStage.Failed, Message = "Plugin build failed." });
        }

        return success;
    }
}
