using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Models;
using UnrealBinaryBuilder.Avalonia.ViewModels;
using Xunit;
using System.Collections.Generic;
using System.IO;
using System;

namespace UnrealBinaryBuilder.Tests;

public class BuildArgumentBuilderTests
{
    [Fact]
    public void BuildEngineArguments_GeneratesCorrectString_ForUE4()
    {
        // Arrange
        var settings = new BuilderSettingsJson
        {
            bWithWin64 = true,
            bWithWin32 = true,
            bWithDDC = true,
            CustomBuildFile = "C:\\Custom.xml"
        };
        var metadata = new UnrealEngineMetadata(4, 22, 0, "4.22", "4.22.0", true, true, true, true, false, false, false, false);
        
        // Act
        var args = BuildArgumentBuilder.BuildEngineArguments(settings, metadata, null);
        string cmd = args.ToString();

        // Assert
        Assert.Contains("-script=\"C:\\Custom.xml\"", cmd);
        Assert.Contains("-set:WithWin32=true", cmd);
        Assert.Contains("-set:WithWin64=true", cmd);
        Assert.Contains("-set:WithDDC=true", cmd);
    }

    [Fact]
    public void BuildEngineArguments_RespectsHostPlatformOnly()
    {
        // Arrange
        var settings = new BuilderSettingsJson
        {
            bHostPlatformOnly = true,
            bWithWin32 = true // Should be ignored
        };
        var metadata = new UnrealEngineMetadata(4, 22, 0, "4.22", "4.22.0", true, true, true, true, false, false, false, false);

        // Act
        var args = BuildArgumentBuilder.BuildEngineArguments(settings, metadata, null);
        string cmd = args.ToString();

        // Assert
        Assert.Contains("-set:HostPlatformOnly=true", cmd);
        Assert.DoesNotContain("-set:WithWin32", cmd);
    }

    [Fact]
    public void BuildEngineArguments_ExcludesWin32_ForUE5()
    {
        // Arrange
        var settings = new BuilderSettingsJson
        {
            bWithWin32 = true
        };
        var metadata = new UnrealEngineMetadata(5, 0, 0, "5.0", "5.0.0", false, false, false, true, true, false, true, true);

        // Act
        var args = BuildArgumentBuilder.BuildEngineArguments(settings, metadata, null);
        string cmd = args.ToString();

        // Assert
        Assert.DoesNotContain("-set:WithWin32", cmd);
    }

    [Fact]
    public void BuildEngineArguments_IncludesVsVersion()
    {
        // Arrange
        var settings = new BuilderSettingsJson();
        var metadata = new UnrealEngineMetadata(5, 0, 0, "5.0", "5.0.0", false, false, false, true, true, false, true, true);
        
        string testDir = Path.Combine(Path.GetTempPath(), "BuildArgumentBuilderTests_" + Guid.NewGuid().ToString());
        string vsPath = Path.Combine(testDir, "2022");
        string editionPath = Path.Combine(vsPath, "Community");
        string msBuildPath = Path.Combine(editionPath, "MSBuild", "Current", "Bin", "amd64");
        Directory.CreateDirectory(msBuildPath);
        File.WriteAllText(Path.Combine(msBuildPath, "msbuild.exe"), "");

        try
        {
            var vsVersion = VisualStudioVersion.ParseVersion(vsPath);
            Assert.NotNull(vsVersion);

            // Act
            var args = BuildArgumentBuilder.BuildEngineArguments(settings, metadata, vsVersion);
            string cmd = args.ToString();

            // Assert
            Assert.Contains("-set:VS2022=true", cmd);
        }
        finally
        {
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    [Fact]
    public void BuildPluginArguments_GeneratesCorrectString()
    {
        // Arrange
        var platformService = new MockPlatformService();
        var plugin = new PluginCardViewModel("C:/Plugins/MyPlugin.uplugin", "C:/Output", "C:/UE5", "UE5", platformService)
        {
            TargetPlatforms = new List<string> { "Win64", "Android" }
        };

        // Act
        var args = BuildArgumentBuilder.BuildPluginArguments(plugin);
        string cmd = args.ToString();

        // Assert
        Assert.Contains("BuildPlugin", cmd);
        Assert.Contains("-Plugin=C:/Plugins/MyPlugin.uplugin", cmd);
        Assert.Contains("-Package=C:/Output", cmd);
        Assert.Contains("-TargetPlatforms=Win64+Android", cmd);
        Assert.Contains("-Rocket", cmd);
    }
}
