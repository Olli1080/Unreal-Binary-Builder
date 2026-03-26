using System;
using System.IO;
using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Models;
using Xunit;

namespace UnrealBinaryBuilder.Tests;

public class UnrealEngineIntegrationTests : IDisposable
{
    private readonly string _mockEnginePath;
    private readonly UnrealEngineProvider _provider;

    public UnrealEngineIntegrationTests()
    {
        _mockEnginePath = Path.Combine(Path.GetTempPath(), "UBB_MockEngine_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_mockEnginePath);
        _provider = new UnrealEngineProvider();
    }

    public void Dispose()
    {
        if (Directory.Exists(_mockEnginePath))
        {
            Directory.Delete(_mockEnginePath, true);
        }
    }

    private void CreateMockVersionFile(int major, int minor, int patch)
    {
        string buildDir = Path.Combine(_mockEnginePath, "Engine", "Build");
        Directory.CreateDirectory(buildDir);
        string versionFile = Path.Combine(buildDir, "Build.version");
        
        string content = $@"{{
            ""MajorVersion"": {major},
            ""MinorVersion"": {minor},
            ""PatchVersion"": {patch},
            ""Changelist"": 0,
            ""CompatibleChangelist"": 0,
            ""IsLicenseeVersion"": 0,
            ""IsPromotedBuild"": 0,
            ""BranchName"": ""UE{major}.{minor}""
        }}";
        
        File.WriteAllText(versionFile, content);
    }

    [Fact]
    public void Provider_DetectsUE4_22_Correctly()
    {
        // Arrange
        CreateMockVersionFile(4, 22, 0);

        // Act
        var metadata = _provider.GetEngineMetadata(_mockEnginePath);

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal(4, metadata.Major);
        Assert.Equal(22, metadata.Minor);
        Assert.True(metadata.SupportWin32);
        Assert.True(metadata.SupportHTML5);
        Assert.True(metadata.SupportConsoles);
        Assert.False(metadata.IsUE5);
    }

    [Fact]
    public void Provider_DetectsUE5_0_Correctly()
    {
        // Arrange
        CreateMockVersionFile(5, 0, 0);

        // Act
        var metadata = _provider.GetEngineMetadata(_mockEnginePath);

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal(5, metadata.Major);
        Assert.Equal(0, metadata.Minor);
        Assert.False(metadata.SupportWin32);
        Assert.False(metadata.SupportHTML5);
        Assert.False(metadata.SupportConsoles);
        Assert.True(metadata.IsUE5);
        Assert.True(metadata.SupportLinuxArm64);
    }

    [Fact]
    public void BuildArgumentBuilder_UE4_22_IncludesWin32()
    {
        // Arrange
        CreateMockVersionFile(4, 22, 0);
        var metadata = _provider.GetEngineMetadata(_mockEnginePath);
        var settings = new BuilderSettingsJson { WithWin32 = true };

        // Act
        var args = BuildArgumentBuilder.BuildEngineArguments(settings, metadata, null);

        // Assert
        Assert.Contains("-set:WithWin32=true", args.ToString());
    }

    [Fact]
    public void BuildArgumentBuilder_UE5_0_ExcludesWin32()
    {
        // Arrange
        CreateMockVersionFile(5, 0, 0);
        var metadata = _provider.GetEngineMetadata(_mockEnginePath);
        var settings = new BuilderSettingsJson { WithWin32 = true };

        // Act
        var args = BuildArgumentBuilder.BuildEngineArguments(settings, metadata, null);

        // Assert
        Assert.DoesNotContain("-set:WithWin32", args.ToString());
    }

    [Fact]
    public void Provider_GetAutomationPath_UE4()
    {
        // Act
        string path = _provider.GetAutomationPath(_mockEnginePath, false);

        // Assert
        Assert.Contains("AutomationToolLauncher.exe", path);
    }

    [Fact]
    public void Provider_GetAutomationPath_UE5()
    {
        // Act
        string path = _provider.GetAutomationPath(_mockEnginePath, true);

        // Assert
        Assert.Contains("AutomationTool/AutomationTool.exe", path);
    }
}
