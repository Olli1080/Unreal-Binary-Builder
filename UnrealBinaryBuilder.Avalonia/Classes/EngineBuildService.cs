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

    public EngineBuildService(
        IProcessExecutor processExecutor, 
        IUBBLogger logger, 
        IZipService zipService,
        IPlatformService platformService,
        IUnrealEngineProvider ueProvider)
    {
        _processExecutor = processExecutor;
        _logger = logger;
        _zipService = zipService;
        _platformService = platformService;
        _ueProvider = ueProvider;
    }

    public string PrepareEngineCommandline(BuilderSettingsJson settings, UnrealEngineMetadata? metadata, VisualStudioVersion? vsVersion)
    {
        return BuildArgumentBuilder.BuildEngineArguments(settings, metadata, vsVersion).ToString();
    }

    public async Task<bool> BuildEngineAsync(string enginePath, BuilderSettingsJson settings, VisualStudioVersion? vsVersion, UnrealEngineMetadata? metadata)
    {
        _logger.Info("Starting Engine Build...", LogCategory.Build);
        GameAnalyticsCSharp.AddDesignEvent("Build:Started");
        GameAnalyticsCSharp.AddProgressStart("Build", "Engine");

        string automationPath = _ueProvider.GetAutomationPath(enginePath, metadata?.IsUE5 ?? false);
        string args = PrepareEngineCommandline(settings, metadata, vsVersion);

        int ec = await _processExecutor.ExecuteAsync(automationPath, args, enginePath, LogCategory.Build);
        bool success = ec == 0;
        
        GameAnalyticsCSharp.AddProgressEnd("Build", "Engine", !success);

        if (success)
        {
            if (settings.bZipEngineBuild && !string.IsNullOrEmpty(settings.ZipEnginePath))
            {
                _logger.Info("Zipping build...", LogCategory.Build);
                GameAnalyticsCSharp.AddDesignEvent("Zip:Started");
                try
                {
                    await _zipService.SaveToZip(Path.Combine(enginePath, "LocalBuilds", "Engine"), settings.ZipEnginePath, settings);
                    GameAnalyticsCSharp.AddDesignEvent("Zip:Finished");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to zip engine build.", LogCategory.Build);
                }
            }

            if (settings.bShutdownIfBuildSuccess && settings.bShutdownPC)
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
        GameAnalyticsCSharp.AddDesignEvent("Shutdown:Started"); 
        _platformService.ShutdownPC(5);
        Environment.Exit(0); 
    }
}
