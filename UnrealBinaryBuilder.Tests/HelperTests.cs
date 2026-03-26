using UnrealBinaryBuilder.Avalonia.Classes;

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
    public void UnrealEngineProvider_ReturnsNull_WhenFileDoesNotExist()
    {
        var provider = new UnrealEngineProvider();
        var metadata = provider.GetEngineMetadata(_testPath);
        Assert.Null(metadata);
    }

    [Fact]
    public void UnrealEngineProvider_ReturnsCorrectMetadata_FromValidFile()
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
        var provider = new UnrealEngineProvider();
        var metadata = provider.GetEngineMetadata(_testPath);

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal("5.3", metadata.VersionString);
        Assert.Equal("5.3.2", metadata.FullVersionString);
        Assert.True(metadata.IsUE5);
    }

    [Fact]
    public void UnrealEngineProvider_ReturnCorrectAutomationPaths()
    {
        string enginePath = "C:/UnrealEngine";
        var provider = new UnrealEngineProvider();
        
        var automationPathUE5 = provider.GetAutomationPath(enginePath, true);
        var automationPathUE4 = provider.GetAutomationPath(enginePath, false);
        var csprojPath = PathHelpers.ToUnixPath(UnrealBinaryBuilderHelpers.GetAutomationToolProjectFile(enginePath) ?? string.Empty);

        Assert.Contains("DotNET/AutomationTool/AutomationTool.exe", automationPathUE5);
        Assert.Contains("DotNET/AutomationToolLauncher.exe", automationPathUE4);
        Assert.Contains("Programs/AutomationTool/AutomationTool.csproj", csprojPath);
    }
}
