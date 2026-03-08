using System;
using System.IO;
using UnrealBinaryBuilder.Avalonia.Classes;
using Xunit;

namespace UnrealBinaryBuilder.Tests;

public class HelperTests : IDisposable
{
    private readonly string _testPath;

    public HelperTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "UBB_Helper_Tests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testRepoPath()); // Just to ensure base exists
    }

    private string _testRepoPath() => _testPath;

    public void Dispose()
    {
        if (Directory.Exists(_testPath))
        {
            Directory.Delete(_testPath, true);
        }
    }

    [Fact]
    public void GetEngineVersion_ReturnsNull_WhenFileDoesNotExist()
    {
        var version = UnrealBinaryBuilderHelpers.GetEngineVersion(_testPath);
        Assert.Null(version);
    }

    [Fact]
    public void GetEngineVersion_ReturnsCorrectVersion_FromValidFile()
    {
        // Arrange
        string engineBuildDir = Path.Combine(_testPath, "Engine", "Build");
        Directory.CreateDirectory(engineBuildDir);
        string versionFile = Path.Combine(engineBuildDir, "Build.version");
        
        string jsonContent = @"{
            ""MajorVersion"": 5,
            ""MinorVersion"": 3,
            ""PatchVersion"": 2,
            ""Changelist"": 0,
            ""CompatibleChangelist"": 0,
            ""IsLicenseeVersion"": 0,
            ""IsPromotedBuild"": 0,
            ""BranchName"": ""++UE5+Release-5.3""
        }";
        File.WriteAllText(versionFile, jsonContent);

        // Act
        var version = UnrealBinaryBuilderHelpers.GetEngineVersion(_testPath);

        // Assert
        Assert.Equal("5.3.2", version);
    }

    [Fact]
    public void PathHelpers_ReturnCorrectPaths()
    {
        string enginePath = "C:\\UnrealEngine";
        
        var automationPathUE5 = UnrealBinaryBuilderHelpers.AutomationPath(enginePath, true);
        var automationPathUE4 = UnrealBinaryBuilderHelpers.AutomationPath(enginePath, false);
        var csprojPath = UnrealBinaryBuilderHelpers.GetAutomationToolProjectFile(enginePath);

        Assert.Contains("DotNET\\AutomationTool\\AutomationTool.exe", automationPathUE5);
        Assert.Contains("DotNET\\AutomationToolLauncher.exe", automationPathUE4);
        Assert.Contains("Programs\\AutomationTool\\AutomationTool.csproj", csprojPath);
    }
}
