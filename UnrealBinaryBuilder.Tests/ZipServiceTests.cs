using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Models;
using Xunit;

namespace UnrealBinaryBuilder.Tests;

public class ZipServiceTests : IDisposable
{
    private readonly string _basePath;
    private readonly string _sourcePath;
    private readonly string _zipPath;
    private readonly ZipService _zipService;

    public ZipServiceTests()
    {
        _basePath = Path.Combine(Path.GetTempPath(), "UBB_ZipTests_" + Guid.NewGuid().ToString());
        _sourcePath = Path.Combine(_basePath, "Source");
        _zipPath = Path.Combine(_basePath, "output.zip");
        
        Directory.CreateDirectory(_sourcePath);
        _zipService = new ZipService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, true);
        }
    }

    private void CreateMockFile(string relativePath)
    {
        string fullPath = Path.Combine(_sourcePath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, "mock-content");
    }

    [Fact]
    public async Task SaveToZip_FiltersPDBs_WhenDisabled()
    {
        // Arrange
        CreateMockFile("Engine/Binaries/Win64/UnrealEditor.exe");
        CreateMockFile("Engine/Binaries/Win64/UnrealEditor.pdb");
        
        var settings = new BuilderSettingsJson { bZipEnginePDB = false };

        // Act
        await _zipService.SaveToZip(_sourcePath, _zipPath, settings);

        // Assert
        using var zip = ZipFile.OpenRead(_zipPath);
        Assert.Contains(zip.Entries, e => e.FullName.EndsWith("UnrealEditor.exe", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(zip.Entries, e => e.FullName.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SaveToZip_IncludesPDBs_WhenEnabled()
    {
        // Arrange
        CreateMockFile("Engine/Binaries/Win64/UnrealEditor.pdb");
        var settings = new BuilderSettingsJson { bZipEnginePDB = true };

        // Act
        await _zipService.SaveToZip(_sourcePath, _zipPath, settings);

        // Assert
        using var zip = ZipFile.OpenRead(_zipPath);
        Assert.Contains(zip.Entries, e => e.FullName.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SaveToZip_FiltersSource_WhenDisabled()
    {
        // Arrange
        CreateMockFile("Engine/Source/Runtime/Core/Core.Build.cs");
        CreateMockFile("Engine/Source/Developer/TargetPlatform/TargetPlatform.Build.cs");
        CreateMockFile("Engine/Config/Base.ini");
        
        var settings = new BuilderSettingsJson { bZipEngineSource = false };

        // Act
        await _zipService.SaveToZip(_sourcePath, _zipPath, settings);

        // Assert
        using var zip = ZipFile.OpenRead(_zipPath);
        Assert.Contains(zip.Entries, e => e.FullName.Replace('\\', '/').Contains("Engine/Config/Base.ini"));
        Assert.DoesNotContain(zip.Entries, e => e.FullName.Replace('\\', '/').Contains("/Source/"));
    }

    [Fact]
    public async Task SaveToZip_FiltersTemplates_WhenDisabled()
    {
        // Arrange
        CreateMockFile("Templates/TP_FirstPerson/Template.png");
        var settings = new BuilderSettingsJson { bZipEngineTemplates = false };

        // Act
        await _zipService.SaveToZip(_sourcePath, _zipPath, settings);

        // Assert
        using var zip = ZipFile.OpenRead(_zipPath);
        Assert.DoesNotContain(zip.Entries, e => e.FullName.Replace('\\', '/').StartsWith("Templates"));
    }

    [Fact]
    public async Task SavePluginToZip_FiltersMarketplaceFiles_WhenEnabled()
    {
        // Arrange
        CreateMockFile("MyPlugin.uplugin");
        CreateMockFile("Binaries/Win64/MyPlugin.dll");
        CreateMockFile("Intermediate/Build/Win64/MyPlugin.obj");
        CreateMockFile("Source/MyPlugin/MyPlugin.Build.cs");

        // Act
        await _zipService.SavePluginToZip(_sourcePath, _zipPath, bZipForMarketplace: true, bFastCompression: true);

        // Assert
        using var zip = ZipFile.OpenRead(_zipPath);
        Assert.Contains(zip.Entries, e => e.FullName == "MyPlugin.uplugin");
        Assert.Contains(zip.Entries, e => e.FullName.Replace('\\', '/').Contains("Source/"));
        Assert.DoesNotContain(zip.Entries, e => e.FullName.Replace('\\', '/').Contains("Binaries/"));
        Assert.DoesNotContain(zip.Entries, e => e.FullName.Replace('\\', '/').Contains("Intermediate/"));
    }

    [Fact]
    public void CanSaveToZip_ValidPath_ReturnsTrue()
    {
        // Act
        bool result = _zipService.CanSaveToZip(_zipPath);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanSaveToZip_InvalidPath_ReturnsFalse()
    {
        // Act
        bool result = _zipService.CanSaveToZip("");

        // Assert
        Assert.False(result);
    }
}
