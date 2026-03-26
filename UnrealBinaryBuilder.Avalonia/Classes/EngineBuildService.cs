using System;
using System.IO;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Classes.Logging;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes;

/// <summary>
/// Provides services for building Unreal Engine from source, including automation command line generation and executing the build process.
/// </summary>
public class EngineBuildService : IEngineBuildService
{
    private readonly IProcessExecutor _processExecutor;
    private readonly IUBBLogger _logger;
    private readonly IZipService _zipService;
    private readonly IPlatformService _platformService;
    private readonly IUnrealEngineProvider _ueProvider;
    private readonly ITelemetryService _telemetry;

    /// <summary>
    /// Initializes a new instance of the <see cref="EngineBuildService"/> class.
    /// </summary>
    /// <param name="processExecutor">The process executor.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="zipService">The zip service.</param>
    /// <param name="platformService">The platform service.</param>
    /// <param name="ueProvider">The Unreal Engine provider.</param>
    /// <param name="telemetry">The telemetry service.</param>
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

    /// <summary>
    /// Prepares the command line arguments for the Unreal Automation Tool (UAT) based on the provided settings.
    /// </summary>
    /// <param name="settings">The builder settings.</param>
    /// <param name="metadata">The Unreal Engine metadata.</param>
    /// <param name="vsVersion">The Visual Studio version to use for the build.</param>
    /// <returns>A string containing the formatted command line arguments.</returns>
    public string PrepareEngineCommandline(BuilderSettingsJson settings, UnrealEngineMetadata? metadata, VisualStudioVersion? vsVersion)
    {
        return BuildArgumentBuilder.BuildEngineArguments(settings, metadata, vsVersion).ToString();
    }

    /// <summary>
    /// Asynchronously builds the Unreal Engine from the specified path using the provided settings.
    /// </summary>
    /// <param name="enginePath">The path to the Unreal Engine source directory.</param>
    /// <param name="settings">The builder settings.</param>
    /// <param name="vsVersion">The target Visual Studio version.</param>
    /// <param name="metadata">The Unreal Engine metadata.</param>
    /// <returns>A task that represents the asynchronous build operation. The task result contains a boolean indicating whether the build was successful.</returns>
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

        if (!success)
        {
            _logger.Error("Engine Build Failed.", LogCategory.Build);
        }

        return success;
    }
}
