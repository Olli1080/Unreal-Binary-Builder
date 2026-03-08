using System;
using System.IO;
using System.Linq;
using UnrealBinaryBuilder.Avalonia.Classes;
using Xunit;

namespace UnrealBinaryBuilder.Tests;

public class VisualStudioTests : IDisposable
{
    private readonly string _testPath;

    public VisualStudioTests()
    {
        _testPath = Path.Combine(Path.GetTempPath(), "UBB_VS_Tests_" + Guid.NewGuid().ToString());
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
    public void VisualStudioConfigurations_DetectsMockedInstallation()
    {
        // Arrange: Create a mock VS structure
        // Root/2022/Community/MSBuild/Current/Bin/amd64/msbuild.exe
        string vsRoot = Path.Combine(_testPath, "Microsoft Visual Studio");
        string versionDir = Path.Combine(vsRoot, "2022");
        string editionDir = Path.Combine(versionDir, "Community");
        string msbuildDir = Path.Combine(editionDir, "MSBuild", "Current", "Bin");
        string x64Dir = Path.Combine(msbuildDir, "amd64");
        string x86Dir = msbuildDir;

        Directory.CreateDirectory(x64Dir);
        File.WriteAllText(Path.Combine(x64Dir, "msbuild.exe"), "dummy");
        File.WriteAllText(Path.Combine(x86Dir, "msbuild.exe"), "dummy");

        // Act
        var configs = new VisualStudioConfigurations(vsRoot);

        // Assert
        Assert.Single(configs.Versions);
        var v2022 = configs.Versions.First();
        Assert.Equal(2022, v2022.Version);
        Assert.Single(v2022.MsBuilds);
        var community = v2022.MsBuilds.First();
        Assert.Equal("Community", community.Edition);
        Assert.Contains("amd64", community.X64Path);
        Assert.Contains("Bin/msbuild.exe", community.X32Path);
    }
}
