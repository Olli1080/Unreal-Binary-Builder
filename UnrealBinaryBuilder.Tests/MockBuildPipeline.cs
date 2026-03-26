using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;
using UnrealBinaryBuilder.Avalonia.ViewModels;

namespace UnrealBinaryBuilder.Tests;

public class MockBuildPipeline : IBuildPipeline
{
    public bool ExecuteEnginePipelineResult { get; set; } = true;
    public bool ExecutePluginPipelineResult { get; set; } = true;

    public Task<bool> ExecuteEnginePipelineAsync(string enginePath, BuilderSettingsJson settings, VisualStudioMsBuild? msBuild, string architecture, VisualStudioVersion? vsVersion, UnrealEngineMetadata? metadata, IProgress<BuildPipelineProgress>? progress = null)
    {
        progress?.Report(new BuildPipelineProgress { Stage = BuildPipelineStage.Finished, Message = "Finished" });
        return Task.FromResult(ExecuteEnginePipelineResult);
    }

    public Task<bool> ExecutePluginPipelineAsync(IEnumerable<PluginCardViewModel> pluginQueue, IProgress<BuildPipelineProgress>? progress = null)
    {
        progress?.Report(new BuildPipelineProgress { Stage = BuildPipelineStage.Finished, Message = "Finished" });
        return Task.FromResult(ExecutePluginPipelineResult);
    }
}
