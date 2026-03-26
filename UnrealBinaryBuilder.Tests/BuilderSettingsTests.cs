using UnrealBinaryBuilder.Avalonia.Classes;

namespace UnrealBinaryBuilder.Tests;

public class BuilderSettingsTests : IDisposable
{
    private readonly string _testPath;
    private readonly SettingsService _settingsService;

    public BuilderSettingsTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "UBB_Tests_" + Guid.NewGuid().ToString());
        _settingsService = new SettingsService(new MockPlatformService(), _testPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testPath))
        {
            Directory.Delete(_testPath, true);
        }
    }

    [Fact]
    public void GetSettings_ReturnsDefaultSettings_WhenNoFileExists()
    {
        // Act
        var settings = _settingsService.GetSettings();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal("Dark", settings.Theme);
        Assert.True(settings.CheckForUpdatesAtStartup);
        Assert.True(File.Exists(Path.Combine(_testPath, "Saved", "Settings.json")));
    }

    [Fact]
    public void SaveSettings_PersistsChanges()
    {
        // Arrange
        var settings = _settingsService.GetSettings();
        settings.Theme = "Light";
        settings.CheckForUpdatesAtStartup = false;

        // Act
        _settingsService.SaveSettings(settings);
        var reloadedSettings = _settingsService.GetSettings();

        // Assert
        Assert.Equal("Light", reloadedSettings.Theme);
        Assert.False(reloadedSettings.CheckForUpdatesAtStartup);
    }

    [Fact]
    public void GitDependencyCache_UsesNewPath_WhenPathIsChanged()
    {
        // Act
        var settings = _settingsService.GetSettings();

        // Assert
        Assert.Contains(PathHelpers.NormalizePath(_testPath), settings.GitDependencyCache);
    }
}
