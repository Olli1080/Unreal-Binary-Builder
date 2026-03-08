using System;
using System.IO;
using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Models;
using Xunit;

namespace UnrealBinaryBuilder.Tests;

public class BuilderSettingsTests : IDisposable
{
    private readonly string _testPath;

    public BuilderSettingsTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "UBB_Tests_" + Guid.NewGuid().ToString());
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
    public void GetSettingsFile_ReturnsDefaultSettings_WhenNoFileExists()
    {
        // Act
        var settings = BuilderSettings.GetSettingsFile();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal("Dark", settings.Theme);
        Assert.True(settings.bCheckForUpdatesAtStartup);
        Assert.True(File.Exists(Path.Combine(_testPath, "Saved", "Settings.json")));
    }

    [Fact]
    public void SaveSettings_PersistsChanges()
    {
        // Arrange
        var settings = BuilderSettings.GetSettingsFile();
        settings.Theme = "Light";
        settings.bCheckForUpdatesAtStartup = false;

        // Act
        BuilderSettings.SaveSettings(settings);
        var reloadedSettings = BuilderSettings.GetSettingsFile();

        // Assert
        Assert.Equal("Light", reloadedSettings.Theme);
        Assert.False(reloadedSettings.bCheckForUpdatesAtStartup);
    }

    [Fact]
    public void GitDependencyCache_UsesNewPath_WhenPathIsChanged()
    {
        // Act
        var settings = BuilderSettings.GetSettingsFile();

        // Assert
        // The default settings in the file might have the hardcoded path from the static initializer if not careful,
        // but our refactor should ensure it uses the current PROGRAM_SAVED_PATH during Generation if we use the property.
        Assert.Contains(PathHelpers.NormalizePath(_testPath), settings.GitDependencyCache);
    }
}
