using System;
using System.IO;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Classes.Logging;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class EngineBuildService : IEngineBuildService
{
    private readonly IProcessExecutor _processExecutor;
    private readonly IUBBLogger _logger;
    private readonly IZipService _zipService;
    private readonly IPlatformService _platformService;
    private readonly IUnrealEngineProvider _ueProvider;
    private readonly ITelemetryService _telemetry;

    public EngineBuildService(
        IProcessExecutor processExecutor, 
        IUBBLogger logger, 
        IZipService zipService,
        IPlatformService platformService,
        IUnrealEngineProvider ueProvider,
        ITelemetryService telemetry)
    {
        _processExecutor = processExecutor;
        _logger = logger;
        _zipService = zipService;
        _platformService = platformService;
        _ueProvider = ueProvider;
        _telemetry = telemetry;
    }

    public string PrepareEngineCommandline(BuilderSettingsJson settings, UnrealEngineMetadata? metadata, VisualStudioVersion? vsVersion)
    {
        return BuildArgumentBuilder.BuildEngineArguments(settings, metadata, vsVersion).ToString();
    }

    public async Task<bool> BuildEngineAsync(string enginePath, BuilderSettingsJson settings, VisualStudioVersion? vsVersion, UnrealEngineMetadata? metadata)
    {
        _logger.Info("Starting Engine Build...", LogCategory.Build);
        _telemetry.TrackEvent(TelemetryConstants.EVENT_BUILD_STARTED);
        _telemetry.TrackProgressStart(TelemetryConstants.CAT_BUILD, TelemetryConstants.STEP_ENGINE);

        string automationPath = _ueProvider.GetAutomationPath(enginePath, metadata?.IsUE5 ?? false);
        string args = PrepareEngineCommandline(settings, metadata, vsVersion);

        int ec = await _processExecutor.ExecuteAsync(automationPath, args, enginePath, LogCategory.Build);
        bool success = ec == 0;
        
        _telemetry.TrackProgressEnd(TelemetryConstants.CAT_BUILD, TelemetryConstants.STEP_ENGINE, !success);

        if (success)
        {
            if (settings.ZipEngineBuild && !string.IsNullOrEmpty(settings.ZipEnginePath))
            {
                _logger.Info("Zipping build...", LogCategory.Build);
                _telemetry.TrackEvent(TelemetryConstants.EVENT_ZIP_STARTED);
                try
                {
                    await _zipService.SaveToZip(Path.Combine(enginePath, "LocalBuilds", "Engine"), settings.ZipEnginePath, settings);
                    _telemetry.TrackEvent(TelemetryConstants.EVENT_ZIP_FINISHED);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to zip engine build.", LogCategory.Build);
                }
            }

            if (settings.ShutdownIfBuildSuccess && settings.ShutdownPC)
            {
                Internal_ShutdownPC();
            }
        }
        else
        {
            _logger.Error("Engine Build Failed.", LogCategory.Build);
        }

        return success;
    }

    private void Internal_ShutdownPC() 
    { 
        _logger.Info("Shutting down PC in 5 seconds...", LogCategory.General); 
        _telemetry.TrackEvent(TelemetryConstants.EVENT_SHUTDOWN_STARTED); 
        _platformService.ShutdownPC(5);
        Environment.Exit(0); 
    }
}
