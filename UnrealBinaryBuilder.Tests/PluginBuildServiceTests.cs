using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.ViewModels;

namespace UnrealBinaryBuilder.Tests;

public class PluginBuildServiceTests : IDisposable
{
    private readonly string _testPath;
    private readonly MockProcessExecutor _processExecutor;
    private readonly MockLogger _logger;
    private readonly MockZipService _zipService;
    private readonly MockTelemetryService _telemetryService;
    private readonly PluginBuildService _buildService;

    public PluginBuildServiceTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "UBB_PluginBuildTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testPath);
        _processExecutor = new MockProcessExecutor();
        _logger = new MockLogger();
        _zipService = new MockZipService();
        _telemetryService = new MockTelemetryService();
        
        _buildService = new PluginBuildService(
            _processExecutor,
            _logger,
            _zipService,
            _telemetryService);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testPath))
        {
            Directory.Delete(_testPath, true);
        }
    }

    [Fact]
    public async Task BuildPluginsAsync_EmptyQueue_ReturnsTrue()
    {
        // Act
        bool result = await _buildService.BuildPluginsAsync(new List<PluginCardViewModel>());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task BuildPluginsAsync_Success_ReturnsTrue()
    {
        // Arrange
        string uplugin = Path.Combine(_testPath, "MyPlugin.uplugin");
        File.WriteAllText(uplugin, "{}");
        
        var mockPlatform = new MockPlatformService();
        var vm = new PluginCardViewModel(uplugin, _testPath, _testPath, "Current", mockPlatform);
        var queue = new List<PluginCardViewModel> { vm };

        // Act
        bool result = await _buildService.BuildPluginsAsync(queue);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task BuildPluginsAsync_Failure_ReturnsFalse()
    {
        // Arrange
        string uplugin = Path.Combine(_testPath, "MyPlugin.uplugin");
        File.WriteAllText(uplugin, "{}");
        
        var mockPlatform = new MockPlatformService();
        var vm = new PluginCardViewModel(uplugin, _testPath, _testPath, "Current", mockPlatform);
        var queue = new List<PluginCardViewModel> { vm };
        
        _processExecutor.ExitCode = 1;

        // Act
        bool result = await _buildService.BuildPluginsAsync(queue);

        // Assert
        Assert.False(result);
    }
}
