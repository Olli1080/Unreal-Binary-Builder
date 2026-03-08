using System;
using System.IO;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;
using UnrealBinaryBuilder.Avalonia.ViewModels;
using Xunit;

namespace UnrealBinaryBuilder.Tests;

public class MockProcessExecutor : IProcessExecutor
{
    public int ExitCode { get; set; } = 0;
    public Task<int> ExecuteAsync(string fileName, string arguments, string workingDirectory = "", Action<string>? onOutput = null, Action<string>? onError = null)
    {
        return Task.FromResult(ExitCode);
    }
}

public class MockUBBUpdater : IUBBUpdater
{
    public event EventHandler<UpdateProgressFinishedEventArgs>? SilentUpdateFinishedEventHandler;
    public event EventHandler<UpdateProgressDownloadEventArgs>? UpdateProgressEventHandler;
    public event EventHandler<UpdateProgressDownloadErrorEventArgs>? UpdateProgressDownloadErrorEventHandler;
    public event EventHandler<UpdateProgressDownloadStartEventArgs>? UpdateDownloadStartedEventHandler;
    public event EventHandler<UpdateProgressDownloadFinishEventArgs>? UpdateDownloadFinishedEventHandler;

    public void CheckForUpdates() { }
    public void CheckForUpdatesSilently() { }
    public void DownloadUpdate() { }
}

public class MockSettingsService : ISettingsService
{
    public BuilderSettingsJson Settings { get; set; } = SettingsService.GetDefaultSettings(Path.Combine(Path.GetTempPath(), "UBB_Mock"));
    public event Action<string, LogMessageType>? OnLog;

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
    private readonly MockUBBUpdater _updater;
    private readonly MockPlatformService _platformService;
    private readonly MockSettingsService _settingsService;
    private readonly MockUnrealEngineProvider _ueProvider;

    public MainWindowViewModelTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "UBB_VM_Tests_" + Guid.NewGuid().ToString());
        _processExecutor = new MockProcessExecutor();
        _updater = new MockUBBUpdater();
        _platformService = new MockPlatformService();
        _settingsService = new MockSettingsService();
        _ueProvider = new MockUnrealEngineProvider();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testPath))
        {
            Directory.Delete(_testPath, true);
        }
    }

    private MainWindowViewModel CreateViewModel() => new MainWindowViewModel(_processExecutor, _updater, _platformService, _settingsService, _ueProvider);

    [Fact]
    public void EnginePath_UpdatesVersionDependencies()
    {
        // Arrange
        var vm = CreateViewModel();
        string engineRoot = Path.Combine(_testPath, "EngineMock");
        Directory.CreateDirectory(engineRoot); // Ensure directory exists
        
        // Mock UE 4.22
        _ueProvider.Metadata = new UnrealEngineMetadata(4, 22, 0, "4.22", "4.22.0", true, true, true, true, false, false, false, false);

        // Act
        vm.EnginePath = engineRoot;

        // Assert
        Assert.True(vm.SupportWin32);
        Assert.True(vm.SupportHTML5);
    }

    [Fact]
    public void PrepareCommandline_GeneratesCorrectString()
    {
        // Arrange
        var vm = CreateViewModel();
        vm.Settings.bWithWin64 = true;
        vm.Settings.bWithWin32 = false;
        vm.Settings.bWithDDC = true;
        vm.Settings.CustomBuildFile = "C:\\CustomBuild.xml";

        // Act
        string cmd = vm.PrepareCommandline();

        // Assert
        Assert.Contains("-script=\"C:\\CustomBuild.xml\"", cmd);
        Assert.Contains("-set:WithDDC=true", cmd);
    }

    [Fact]
    public void PrepareCommandline_RespectsVersionDependencies()
    {
        // Arrange
        var vm = CreateViewModel();
        string engineRoot = Path.Combine(_testPath, "EngineMock_422");
        Directory.CreateDirectory(engineRoot); // Ensure directory exists
        
        // UE 4.22
        _ueProvider.Metadata = new UnrealEngineMetadata(4, 22, 0, "4.22", "4.22.0", true, true, true, true, false, false, false, false);
        vm.EnginePath = engineRoot;
        vm.Settings.bWithWin32 = true;

        // Act
        string cmd = vm.PrepareCommandline();

        // Assert
        Assert.Contains("-set:WithWin32=true", cmd);

        // Switch to UE 5.0
        _ueProvider.Metadata = new UnrealEngineMetadata(5, 0, 0, "5.0", "5.0.0", false, false, false, true, true, false, true, true);
        vm.EnginePath = engineRoot; // Trigger update

        // Act
        cmd = vm.PrepareCommandline();

        // Assert
        Assert.DoesNotContain("-set:WithWin32", cmd);
    }
}
