using System;
using System.IO;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes;
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

public class MainWindowViewModelTests : IDisposable
{
    private readonly string _testPath;

    public MainWindowViewModelTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "UBB_VM_Tests_" + Guid.NewGuid().ToString());
        BuilderSettings.SetProgramSavedPath(_testPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testPath))
        {
            Directory.Delete(_testPath, true);
        }
    }

    [Fact]
    public void EnginePath_UpdatesVersionDependencies()
    {
        // Arrange
        var vm = new MainWindowViewModel(new MockProcessExecutor());
        string engineRoot = Path.Combine(_testPath, "EngineMock");
        string versionFile = Path.Combine(engineRoot, "Engine", "Build", "Build.version");
        Directory.CreateDirectory(Path.GetDirectoryName(versionFile)!);
        
        // Mocking UE 4.22 (should support Win32)
        File.WriteAllText(versionFile, "{\"MajorVersion\": 4, \"MinorVersion\": 22, \"PatchVersion\": 0}");

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
        var vm = new MainWindowViewModel(new MockProcessExecutor());
        vm.Settings.bWithWin64 = true;
        vm.Settings.bWithWin32 = false;
        vm.Settings.bWithDDC = true;
        vm.Settings.CustomBuildFile = "C:\\CustomBuild.xml";

        // Act
        string cmd = vm.PrepareCommandline();

        // Assert
        Assert.Contains("-script=\"C:\\CustomBuild.xml\"", cmd);
        Assert.Contains("-set:WithDDC=true", cmd);
        // If UE version is not set, it might default to newer which doesn't have Win32
        // Let's force an old version to see if Win32 appears/disappears
    }

    [Fact]
    public void PrepareCommandline_RespectsVersionDependencies()
    {
        // Arrange
        var vm = new MainWindowViewModel(new MockProcessExecutor());
        string engineRoot = Path.Combine(_testPath, "EngineMock_422");
        string versionFile = Path.Combine(engineRoot, "Engine", "Build", "Build.version");
        Directory.CreateDirectory(Path.GetDirectoryName(versionFile)!);
        File.WriteAllText(versionFile, "{\"MajorVersion\": 4, \"MinorVersion\": 22, \"PatchVersion\": 0}");
        
        vm.EnginePath = engineRoot;
        vm.Settings.bWithWin32 = true;

        // Act
        string cmd = vm.PrepareCommandline();

        // Assert
        Assert.Contains("-set:WithWin32=true", cmd);

        // Switch to UE 5.0
        File.WriteAllText(versionFile, "{\"MajorVersion\": 5, \"MinorVersion\": 0, \"PatchVersion\": 0}");
        vm.EnginePath = engineRoot; // Trigger update

        // Act
        cmd = vm.PrepareCommandline();

        // Assert
        Assert.DoesNotContain("-set:WithWin32", cmd);
    }
}
