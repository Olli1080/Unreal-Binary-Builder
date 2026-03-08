using System;
using System.IO;
using UnrealBinaryBuilder.Avalonia.ViewModels;
using Xunit;

namespace UnrealBinaryBuilder.Tests;

public class PluginCardViewModelTests : IDisposable
{
    private readonly string _testPath;

    public PluginCardViewModelTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "UBB_Plugin_Tests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testPath))
        {
            Directory.Delete(_testPath, true);
        }
    }

    [Fact]
    public void PluginCard_LoadsDataCorrectly()
    {
        // Arrange
        string pluginFile = Path.Combine(_testPath, "TestPlugin.uplugin");
        File.WriteAllText(pluginFile, "{\"Description\": \"A test plugin description.\"}");
        string enginePath = Path.Combine(_testPath, "Engine");
        string outPath = Path.Combine(_testPath, "Out");

        // Act
        var vm = new PluginCardViewModel(pluginFile, outPath, enginePath, "5.3", new MockPlatformService());

        // Assert
        Assert.Equal("TestPlugin", vm.PluginName);
        Assert.Equal("A test plugin description.", vm.PluginDescription);
        Assert.False(vm.IsLoading);
    }

    [Fact]
    public void PluginCard_StateTransitions()
    {
        // Arrange
        string pluginFile = Path.Combine(_testPath, "test.uplugin");
        string enginePath = Path.Combine(_testPath, "Engine");
        string outPath = Path.Combine(_testPath, "Out");
        var vm = new PluginCardViewModel(pluginFile, outPath, enginePath, "5.3", new MockPlatformService());

        // Act & Assert (Start)
        vm.StartBuild();
        Assert.True(vm.IsLoading);
        Assert.False(vm.IsSuccess);

        // Act & Assert (Finish Success)
        vm.FinishBuild(true);
        Assert.False(vm.IsLoading);
        Assert.True(vm.IsSuccess);
        Assert.False(vm.IsFailed);

        // Act & Assert (Finish Failure)
        vm.FinishBuild(false);
        Assert.False(vm.IsLoading);
        Assert.False(vm.IsSuccess);
        Assert.True(vm.IsFailed);
    }
}
