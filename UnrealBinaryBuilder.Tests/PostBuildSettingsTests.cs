using System.IO.Compression;
using UnrealBinaryBuilder.Avalonia.Classes;

namespace UnrealBinaryBuilder.Tests;

public class PostBuildSettingsTests : IDisposable
{
    private readonly string _testDir;
    private readonly PostBuildSettings _postBuild;

    public PostBuildSettingsTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "UBB_PostBuild_Tests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);
        _postBuild = new PostBuildSettings();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }

    [Fact]
    public void DirectoryIsWritable_ReturnsTrue_ForValidDirectory()
    {
        Assert.True(_postBuild.DirectoryIsWritable(_testDir));
    }

    [Fact]
    public void DirectoryIsWritable_ReturnsFalse_ForNonExistentDirectory()
    {
        Assert.False(_postBuild.DirectoryIsWritable(Path.Combine(_testDir, "DoesNotExist")));
    }

    [Fact]
    public void BytesToString_ReturnsCorrectFormat()
    {
        Assert.Equal("1KB", PostBuildSettings.BytesToString(1024));
        Assert.Equal("1MB", PostBuildSettings.BytesToString(1024 * 1024));
        Assert.Equal("0B", PostBuildSettings.BytesToString(0));
    }

    [Fact]
    public async Task SaveToZip_RespectsSkipSettings()
    {
        // Arrange
        string sourceDir = Path.Combine(_testDir, "Source");
        string zipFile = Path.Combine(_testDir, "test.zip");
        Directory.CreateDirectory(sourceDir);
        
        // Create some files
        File.WriteAllText(Path.Combine(sourceDir, "test.pdb"), "pdb content");
        File.WriteAllText(Path.Combine(sourceDir, "test.exe"), "exe content");
        Directory.CreateDirectory(Path.Combine(sourceDir, "Samples"));
        File.WriteAllText(Path.Combine(sourceDir, "Samples", "sample.txt"), "sample content");

        var settings = SettingsService.GetDefaultSettings(_testDir);
        settings.ZipEnginePDB = false; // Skip PDBs
        settings.ZipEngineSamples = false; // Skip Samples

        // Act
        _postBuild.PrepareToSave();
        await _postBuild.SaveToZip(sourceDir, zipFile, settings);

        // Assert
        using (var archive = ZipFile.OpenRead(zipFile))
        {
            Assert.Single(archive.Entries);
            Assert.Equal("test.exe", archive.Entries[0].FullName);
            Assert.DoesNotContain(archive.Entries, e => e.FullName.EndsWith(".pdb"));
            Assert.DoesNotContain(archive.Entries, e => e.FullName.Contains("Samples"));
        }
    }
}
