using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Classes.Logging;
using System;

namespace UnrealBinaryBuilder.Avalonia.Classes;

/// <summary>
/// Implementation of the application initialization sequence.
/// </summary>
public class AppInitializer : IAppInitializer
{
    private readonly ITelemetryService _telemetryService;
    private readonly ISettingsService _settingsService;
    private readonly IVelopackUpdaterService _updater;
    private readonly IUIService _uiService;
    private readonly IUBBLogger _logger;

    public AppInitializer(
        ITelemetryService telemetryService,
        ISettingsService settingsService,
        IVelopackUpdaterService updater,
        IUIService uiService,
        IUBBLogger logger)
    {
        _telemetryService = telemetryService;
        _settingsService = settingsService;
        _updater = updater;
        _uiService = uiService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        // 0. Log System Info
        _logger.Info($"Application Starting: Unreal Binary Builder {UnrealBinaryBuilderHelpers.GetProductVersionString()}", LogCategory.General);
        _logger.Info($"OS: {Environment.OSVersion}", LogCategory.General);
        _logger.Info($".NET Runtime: {Environment.Version}", LogCategory.General);
        _logger.Info($"CPU Count: {Environment.ProcessorCount}", LogCategory.General);

        // 1. Initialize Telemetry
        _telemetryService.Initialize(UnrealBinaryBuilderHelpers.GetProductVersionString());

        // 2. Load Settings
        var settings = _settingsService.GetSettings();

        // 3. Apply Theme
        _uiService.ApplyTheme(settings.Theme);

        // 4. Check for Updates if enabled
        if (settings.CheckForUpdatesAtStartup)
        {
            // We don't want to await this and block the UI if it's slow, 
            // but we can fire it off here.
            _ = _updater.CheckForUpdatesAsync(true);
        }

        await Task.CompletedTask;
    }
}
