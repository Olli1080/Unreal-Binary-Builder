using System;
using System.IO;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Classes.Logging;
using UnrealBinaryBuilder.Avalonia.Models;
using UnrealBinaryBuilder.Avalonia.ViewModels;
using Xunit;

namespace UnrealBinaryBuilder.Tests;

public class MockProcessExecutor : IProcessExecutor
{
    public int ExitCode { get; set; } = 0;
    public Task<int> ExecuteAsync(string fileName, string arguments, string workingDirectory = "", LogCategory category = LogCategory.Build)
    {
        return Task.FromResult(ExitCode);
    }
}

public class MockLogger : IUBBLogger
{
    public void Log(string message, LogLevel level = LogLevel.Info, LogCategory category = LogCategory.General) { }
    public void Debug(string message, LogCategory category = LogCategory.General) { }
    public void Info(string message, LogCategory category = LogCategory.General) { }
    public void Success(string message, LogCategory category = LogCategory.General) { }
    public void Warning(string message, LogCategory category = LogCategory.General) { }
    public void Error(string message, LogCategory category = LogCategory.General) { }
    public void Error(Exception exception, string? message = null, LogCategory category = LogCategory.General) { }
}

public class MockVelopackUpdaterService : IVelopackUpdaterService
{
    public bool IsUpdateAvailable { get; set; } = false;
    public Task CheckForUpdatesAsync(bool silent = false) => Task.CompletedTask;
    public Task DownloadUpdatesAsync() => Task.CompletedTask;
    public void ApplyUpdatesAndRestart() { }
}

public class MockSettingsService : ISettingsService
{
    public BuilderSettingsJson Settings { get; set; } = SettingsService.GetDefaultSettings(Path.Combine(Path.GetTempPath(), "UBB_Mock"));
#pragma warning disable CS0067
    public event Action<string, LogMessageType>? OnLog;
#pragma warning restore CS0067

    public BuilderSettingsJson GetSettings() => Settings;
    public void SaveSettings(BuilderSettingsJson settings) => Settings = settings;
    public void WriteToLogFile(string content) { }
    public void WriteErrorsToLogFile(string content) { }
    public void OpenLogFolder() { }
    public void OpenSettings() { }
}

public class MockUnrealEngineProvider : IUnrealEngineProvider
{
    public UnrealEngineMetadata? Metadata { get; set; }
    public string AutomationPath { get; set; } = "RunUAT.bat";

    public UnrealEngineMetadata? GetEngineMetadata(string enginePath) => Metadata;
    public string GetAutomationPath(string enginePath, bool isUE5) => AutomationPath;
}

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
        _logFormatter);

    [Fact]
    public async Task ShowToast_TriggersUiService()
    {
        // Arrange
        var vm = CreateViewModel();
        string receivedMessage = string.Empty;
        UBBNotificationType receivedType = UBBNotificationType.Info;
        
        _uiService.ShowNotification += (s, e) => {
            receivedMessage = e.Message;
            receivedType = e.Type;
        };

        // Act
        // CopyCommandLine triggers a toast
        await vm.CopyCommandLineCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal("Command line copied to clipboard!", receivedMessage);
        Assert.Equal(UBBNotificationType.Info, receivedType);
    }

    [Fact]
    public void EnginePath_UpdatesVersionDependencies()
    {
        // Arrange
        var vm = CreateViewModel();
        string engineRoot = Path.Combine(_testPath, "EngineMock");
        Directory.CreateDirectory(engineRoot); // Ensure directory exists
        
        // Mock UE 4.22
        _ueProvider.Metadata = new UnrealEngineMetadata(4, 22, 0, "4.22", "4.22.0", true, true, true, true, false, false, false, false);
        _gitService.GitInfo = "Branch: master | Hash: 12345";

        // Act
        vm.EnginePath = engineRoot;

        // Assert
        Assert.True(vm.SupportWin32);
        Assert.True(vm.SupportHTML5);
        Assert.Equal("Branch: master | Hash: 12345", vm.GitInfo);
    }

    [Fact]
    public void PrepareCommandline_GeneratesCorrectString()
    {
        // Arrange
        var vm = CreateViewModel();
        _engineBuildService.PrepareEngineCommandlineResult = "-mock-args";

        // Act
        string cmd = vm.PrepareCommandline();

        // Assert
        Assert.Equal("-mock-args", cmd);
    }

    [Fact]
    public void TimerService_UpdatesElapsedTime()
    {
        // Arrange
        var vm = CreateViewModel();
        
        // Act
        _timerService.TriggerElapsedChanged("00:01:23");

        // Assert
        Assert.Equal("00:01:23", vm.ElapsedTime);
    }
}
