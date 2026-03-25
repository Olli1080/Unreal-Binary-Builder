using GameAnalyticsSDK.Net;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Classes.Logging;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class GameAnalyticsTelemetryService : ITelemetryService
{
    private readonly IUBBLogger _logger;
    private bool _isInitialized = false;

    // Please DO NOT change this. If you don't want Analytics, set both GAME_KEY and SECRET_KEY to null ///////////////////
    private static readonly string? GAME_KEY = null;
    private static readonly string? SECRET_KEY = null;
    ///////////////////////////////////////////////////////////////////////////////////////////////

    public GameAnalyticsTelemetryService(IUBBLogger logger)
    {
        _logger = logger;
    }

    public void Initialize(string productVersion)
    {
        if (GAME_KEY == null || SECRET_KEY == null)
        {
            _logger.Info("Telemetry is disabled (Keys are null).", LogCategory.Telemetry);
            return;
        }

        GameAnalytics.ConfigureBuild($"Unreal Binary Builder {productVersion}");
        GameAnalytics.Initialize(GAME_KEY, SECRET_KEY);
        _isInitialized = true;
        _logger.Info("Telemetry Initialized (GameAnalytics).", LogCategory.Telemetry);

#if DEBUG
        TrackEvent(TelemetryConstants.EVENT_PROGRAM_START_DEBUG);
#else
        TrackEvent(TelemetryConstants.EVENT_PROGRAM_START_RELEASE);
#endif
    }

    public void TrackEvent(string eventName)
    {
        if (!_isInitialized) return;

        GameAnalytics.AddDesignEvent(eventName);
#if DEBUG
        _logger.Info($"Telemetry (Design): {eventName}", LogCategory.Telemetry);
#endif
    }

    public void TrackProgressStart(string category, string step)
    {
        if (!_isInitialized) return;

        GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Start, category, step);
#if DEBUG
        _logger.Info($"Telemetry (Progress Start): {category}::{step}", LogCategory.Telemetry);
#endif
    }

    public void TrackProgressEnd(string category, string step, bool isFailure = false)
    {
        if (!_isInitialized) return;

        var status = isFailure ? EGAProgressionStatus.Fail : EGAProgressionStatus.Complete;
        GameAnalytics.AddProgressionEvent(status, category, step);
#if DEBUG
        _logger.Info($"Telemetry (Progress End): {category}::{step} (Success: {!isFailure})", LogCategory.Telemetry);
#endif
    }

    public void TrackError(string message, TelemetrySeverity severity)
    {
        if (!_isInitialized) return;

        EGAErrorSeverity gaSeverity = severity switch
        {
            TelemetrySeverity.Info => EGAErrorSeverity.Info,
            TelemetrySeverity.Warning => EGAErrorSeverity.Warning,
            TelemetrySeverity.Error => EGAErrorSeverity.Error,
            TelemetrySeverity.Critical => EGAErrorSeverity.Critical,
            _ => EGAErrorSeverity.Error
        };

        GameAnalytics.AddErrorEvent(gaSeverity, message);
#if DEBUG
        _logger.Info($"Telemetry (Error): [{gaSeverity}] {message}", LogCategory.Telemetry);
#endif
    }

    public void Shutdown()
    {
        if (!_isInitialized) return;

        GameAnalytics.EndSession();
        _isInitialized = false;
        _logger.Info("Telemetry Shutdown.", LogCategory.Telemetry);
    }
}
