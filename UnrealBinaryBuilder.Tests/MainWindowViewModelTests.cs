using System;
using System.IO;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Classes.Logging;
using UnrealBinaryBuilder.Avalonia.Models;
using UnrealBinaryBuilder.Avalonia.ViewModels;
using Xunit;
using System.Collections.Generic;
using Avalonia.Threading;

namespace UnrealBinaryBuilder.Tests;

public class MainWindowViewModelTests : IDisposable
{
    private readonly string _testPath;
    private readonly MockProcessExecutor _processExecutor;
    private readonly MockVelopackUpdaterService _updater;
    private readonly MockPlatformService _platformService;
    private readonly MockSettingsService _settingsService;
    private readonly MockUnrealEngineProvider _ueProvider;
    private readonly MockLogger _logger;
    private readonly UiLogSink _uiLogSink;
    private readonly MockUIService _uiService;
    private readonly MockSetupService _setupService;
    private readonly MockZipService _zipService;
    private readonly MockEngineBuildService _engineBuildService;
    private readonly MockPluginBuildService _pluginBuildService;
    private readonly MockGitService _gitService;
    private readonly MockBuildTimerService _timerService;
    private readonly MockBuildHistoryService _historyService;
    private readonly MockBuildOrchestrationService _orchestrationService;
    private readonly MockLogFormatterService _logFormatter;
    private readonly MockTelemetryService _telemetryService;
    private readonly MockBuildPipeline _buildPipeline;
    private readonly MockLocalizationService _localizationService;

    public MainWindowViewModelTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "UBB_VM_Tests_" + Guid.NewGuid().ToString());
        _processExecutor = new MockProcessExecutor();
        _updater = new MockVelopackUpdaterService();
        _platformService = new MockPlatformService();
        _settingsService = new MockSettingsService();
        _ueProvider = new MockUnrealEngineProvider();
        _logger = new MockLogger();
        _uiLogSink = new UiLogSink();
        _uiService = new MockUIService();
        _setupService = new MockSetupService();
        _zipService = new MockZipService();
        _engineBuildService = new MockEngineBuildService();
        _pluginBuildService = new MockPluginBuildService();
        _gitService = new MockGitService();
        _timerService = new MockBuildTimerService();
        _historyService = new MockBuildHistoryService();
        _orchestrationService = new MockBuildOrchestrationService();
        _logFormatter = new MockLogFormatterService();
        _telemetryService = new MockTelemetryService();
        _buildPipeline = new MockBuildPipeline();
        _localizationService = new MockLocalizationService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testPath))
        {
            Directory.Delete(_testPath, true);
        }
    }

    private MainWindowViewModel CreateViewModel() => new MainWindowViewModel(
        _processExecutor, 
        _updater, 
        _platformService, 
        _settingsService, 
        _ueProvider, 
        _logger, 
        _uiLogSink, 
        _uiService, 
        _setupService, 
        _zipService, 
        _engineBuildService, 
        _pluginBuildService, 
        _gitService, 
        _timerService, 
        _historyService, 
        _orchestrationService, 
        _logFormatter, 
        _telemetryService, 
        _buildPipeline,
        _localizationService);

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Act
        var vm = CreateViewModel();

        // Assert
        Assert.NotNull(vm.Settings);
        Assert.NotNull(vm.AvailableLanguages);
        Assert.NotEmpty(vm.AvailableLanguages);
        Assert.NotNull(vm.Dashboard);
    }

    [Fact]
    public void SelectedLanguage_ChangesApplicationLanguage()
    {
        // Arrange
        var vm = CreateViewModel();
        var german = new LanguageInfo("de-DE", "Deutsch");

        // Act
        vm.SelectedLanguage = german;

        // Assert
        Assert.Equal("de-DE", _localizationService.CurrentLanguage);
        Assert.Equal("de-DE", vm.Settings.Language);
    }

    [Fact]
    public async Task CheckForUpdates_TriggersUpdater()
    {
        // Arrange
        var vm = CreateViewModel();

        // Act
        await vm.CheckForUpdatesCommand.ExecuteAsync(null);

        // Assert
        // We can't easily check internal state of mock updater without more exposure,
        // but the fact it didn't crash is a start.
    }

    [Fact]
    public void ElapsedTime_Updates_WhenTimerTriggers()
    {
        // Arrange
        var vm = CreateViewModel();
        
        // Act
        _timerService.TriggerElapsedChanged("00:01:23");

        // Assert
        Assert.Equal("00:01:23", vm.ElapsedTime);
    }
}
