using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;
using Xunit;

namespace UnrealBinaryBuilder.Tests;

public class SetupServiceTests : IDisposable
{
    private readonly string _testPath;
    private readonly MockProcessExecutor _processExecutor;
    private readonly MockLogger _logger;
    private readonly SetupService _setupService;

    public SetupServiceTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "UBB_SetupTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testPath);
        _processExecutor = new MockProcessExecutor();
        _logger = new MockLogger();
        _setupService = new SetupService(_processExecutor, _logger);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testPath))
        {
            Directory.Delete(_testPath, true);
        }
    }

    // Helper to create VisualStudioMsBuild via reflection since it has no public constructor
    private VisualStudioMsBuild CreateMsBuild(string edition, string x64, string x32)
    {
        var msBuild = (VisualStudioMsBuild)Activator.CreateInstance(typeof(VisualStudioMsBuild), true)!;
        typeof(VisualStudioMsBuild).GetField("_edition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(msBuild, edition);
        typeof(VisualStudioMsBuild).GetField("_x64", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(msBuild, x64);
        typeof(VisualStudioMsBuild).GetField("_x32", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(msBuild, x32);
        return msBuild;
    }

    [Fact]
    public void PrepareSetupArgs_WithAllIncluded_ContainsAll()
    {
        // Arrange
        var settings = SettingsService.GetDefaultSettings(_testPath);
        settings.GitDependencyAll = true;

        // Act
        string args = _setupService.PrepareSetupArgs(settings);

        // Assert
        Assert.Contains("--all", args);
        Assert.Contains("--force", args);
    }

    [Fact]
    public void PrepareSetupArgs_WithExclusions_ContainsExcludes()
    {
        // Arrange
        var settings = SettingsService.GetDefaultSettings(_testPath);
        settings.GitDependencyPlatforms = new List<GitPlatform>
        {
            new GitPlatform("Win64", false),
            new GitPlatform("Android", true)
        };

        // Act
        string args = _setupService.PrepareSetupArgs(settings);

        // Assert
        Assert.Contains("--exclude=Win64", args);
        Assert.DoesNotContain("--exclude=Android", args);
    }

    [Fact]
    public void PrepareSetupArgs_WithNoCache_ContainsNoCache()
    {
        // Arrange
        var settings = SettingsService.GetDefaultSettings(_testPath);
        settings.GitDependencyEnableCache = false;

        // Act
        string args = _setupService.PrepareSetupArgs(settings);

        // Assert
        Assert.Contains("--no-cache", args);
    }

    [Fact]
    public void PrepareSetupArgs_WithCache_ContainsCacheArgs()
    {
        // Arrange
        var settings = SettingsService.GetDefaultSettings(_testPath);
        settings.GitDependencyEnableCache = true;
        settings.GitDependencyCache = "C:\\Cache";
        settings.GitDependencyCacheMultiplier = 2.5;

        // Act
        string args = _setupService.PrepareSetupArgs(settings);

        // Assert
        Assert.Contains("--cache=C:/Cache", args);
        Assert.Contains("--cache-size-multiplier=2.5", args);
    }

    [Fact]
    public void PrepareSetupArgs_WithProxy_ContainsProxy()
    {
        // Arrange
        var settings = SettingsService.GetDefaultSettings(_testPath);
        settings.GitDependencyProxy = "http://proxy:8080";

        // Act
        string args = _setupService.PrepareSetupArgs(settings);

        // Assert
        Assert.Contains("--proxy=http://proxy:8080", args);
    }

    [Fact]
    public async Task RunSetupChainAsync_FullSuccess_ReturnsTrue()
    {
        // Arrange
        var settings = SettingsService.GetDefaultSettings(_testPath);
        settings.bBuildSetupBatFile = true;
        settings.bGenerateProjectFiles = true;
        settings.bBuildAutomationTool = true;
        
        var msBuild = CreateMsBuild("Mock", "path/x64", "path/x32");
        
        // Mock AutomationTool.sln existence
        string slnPath = Path.Combine(_testPath, "Engine", "Source", "Programs", "AutomationTool", "AutomationTool.sln");
        Directory.CreateDirectory(Path.GetDirectoryName(slnPath)!);
        File.WriteAllText(slnPath, "mock-sln");

        // Act
        bool result = await _setupService.RunSetupChainAsync(_testPath, settings, msBuild, "x64");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task RunSetupChainAsync_SetupFails_StopsChain()
    {
        // Arrange
        var settings = SettingsService.GetDefaultSettings(_testPath);
        settings.bBuildSetupBatFile = true;
        settings.bGenerateProjectFiles = true;
        
        _processExecutor.ExitCode = 1; // Simulate failure

        // Act
        bool result = await _setupService.RunSetupChainAsync(_testPath, settings, null, "x64");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RunSetupChainAsync_BatFilesMissing_ReportsError()
    {
        // Arrange
        var settings = SettingsService.GetDefaultSettings(_testPath);
        settings.bBuildAutomationTool = true;
        var msBuild = CreateMsBuild("Mock", "path/x64", "path/x32");
        
        // Don't create the .sln file

        // Act
        bool result = await _setupService.RunSetupChainAsync(_testPath, settings, msBuild, "x64");

        // Assert
        Assert.False(result);
    }
}
