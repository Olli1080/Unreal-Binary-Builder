using System.IO;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Classes.Logging;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class SetupService : ISetupService
{
    private readonly IProcessExecutor _processExecutor;
    private readonly IUBBLogger _logger;
    private readonly ITelemetryService _telemetry;

    public SetupService(IProcessExecutor processExecutor, IUBBLogger logger, ITelemetryService telemetry)
    {
        _processExecutor = processExecutor;
        _logger = logger;
        _telemetry = telemetry;
    }

    public string PrepareSetupArgs(BuilderSettingsJson settings)
    {
        string args = "--force";
        if (settings.GitDependencyAll) args += " --all";
        foreach (var gp in settings.GitDependencyPlatforms)
        {
            if (!gp.IsIncluded) args += $" --exclude={gp.Name}";
        }
        
        args += $" --threads={settings.GitDependencyThreads} --max-retries={settings.GitDependencyMaxRetries}";
        
        if (!settings.GitDependencyEnableCache) args += " --no-cache";
        else if (!string.IsNullOrEmpty(settings.GitDependencyCache))
        {
            args += $" --cache={PathHelpers.ToUnixPath(settings.GitDependencyCache)} --cache-size-multiplier={settings.GitDependencyCacheMultiplier.ToString(System.Globalization.CultureInfo.InvariantCulture)} --cache-days={settings.GitDependencyCacheDays}";
        }
        
        if (!string.IsNullOrEmpty(settings.GitDependencyProxy)) args += $" --proxy={settings.GitDependencyProxy}";
        
        return args;
    }

    public async Task<int> RunSetupAsync(string enginePath, BuilderSettingsJson settings)
    {
        _logger.Info("Running Setup.bat...", LogCategory.Build);
        _telemetry.TrackProgressStart(TelemetryConstants.CAT_BUILD, "Setup");
        
        string setupPath = Path.Combine(enginePath, "Setup.bat");
        string args = PrepareSetupArgs(settings);
        
        int ec = await _processExecutor.ExecuteAsync(setupPath, args, enginePath, LogCategory.Build);
        
        _telemetry.TrackProgressEnd(TelemetryConstants.CAT_BUILD, "Setup", ec != 0);
        return ec;
    }

    public async Task<int> GenerateProjectFilesAsync(string enginePath)
    {
        _logger.Info("Generating Project Files...", LogCategory.Build);
        _telemetry.TrackProgressStart(TelemetryConstants.CAT_BUILD, "ProjectFiles");
        
        string gpfPath = Path.Combine(enginePath, "GenerateProjectFiles.bat");
        int ec = await _processExecutor.ExecuteAsync(gpfPath, string.Empty, enginePath, LogCategory.Build);
        
        _telemetry.TrackProgressEnd(TelemetryConstants.CAT_BUILD, "ProjectFiles", ec != 0);
        return ec;
    }

    public async Task<int> BuildAutomationToolAsync(string enginePath, VisualStudioMsBuild msBuild, string architecture)
    {
        _logger.Info("Building AutomationTool...", LogCategory.Build);
        _telemetry.TrackProgressStart(TelemetryConstants.CAT_BUILD, "AutomationTool");
        
        string msbuildPath = architecture == "x64" ? msBuild.X64Path : msBuild.X32Path;
        string slnPath = Path.Combine(enginePath, "Engine", "Source", "Programs", "AutomationTool", "AutomationTool.sln");
        
        if (!File.Exists(slnPath))
        {
            _logger.Error($"AutomationTool.sln not found at: {slnPath}", LogCategory.Build);
            _telemetry.TrackProgressEnd(TelemetryConstants.CAT_BUILD, "AutomationTool", true);
            return -1;
        }

        int ec = await _processExecutor.ExecuteAsync(msbuildPath, $"\"{slnPath}\" /p:Configuration=Development /p:Platform=AnyCPU", enginePath, LogCategory.Build);
        
        _telemetry.TrackProgressEnd(TelemetryConstants.CAT_BUILD, "AutomationTool", ec != 0);
        return ec;
    }

    public async Task<bool> RunSetupChainAsync(string enginePath, BuilderSettingsJson settings, VisualStudioMsBuild? msBuild, string architecture)
    {
        _logger.Info($"Starting Build Chain in: {enginePath}", LogCategory.Build);
        
        if (settings.BuildSetupBatFile)
        {
            int ec = await RunSetupAsync(enginePath, settings);
            if (ec != 0) return false;
        }

        if (settings.GenerateProjectFiles)
        {
            int ec = await GenerateProjectFilesAsync(enginePath);
            if (ec != 0) return false;
        }

        if (settings.BuildAutomationTool)
        {
            if (msBuild == null)
            {
                _logger.Error("MSBuild not selected. Skipping AutomationTool build.", LogCategory.Build);
                return false;
            }
            
            int ec = await BuildAutomationToolAsync(enginePath, msBuild, architecture);
            if (ec != 0) return false;
        }

        return true;
    }
}
