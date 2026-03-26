using System;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

/// <summary>
/// Defines the sequence of operations for an engine or plugin build.
/// </summary>
public enum BuildPipelineStage
{
    Idle,
    Setup,
    ProjectFiles,
    AutomationTool,
    EngineBuild,
    PluginBuild,
    Zipping,
    Finished,
    Failed
}

/// <summary>
/// Progress information for the entire build pipeline.
/// </summary>
public class BuildPipelineProgress
{
    public BuildPipelineStage Stage { get; set; }
    public string Message { get; set; } = string.Empty;
    public double OverallPercentage { get; set; }
    public double SubStagePercentage { get; set; }
}

/// <summary>
/// Service responsible for orchestrating the multi-step build process.
/// </summary>
public interface IBuildPipeline
{
    /// <summary>
    /// Executes the full engine build pipeline based on settings.
    /// </summary>
    Task<bool> ExecuteEnginePipelineAsync(string enginePath, BuilderSettingsJson settings, VisualStudioMsBuild? msBuild, string architecture, VisualStudioVersion? vsVersion, UnrealEngineMetadata? metadata, IProgress<BuildPipelineProgress>? progress = null);

    /// <summary>
    /// Executes the plugin build pipeline.
    /// </summary>
    Task<bool> ExecutePluginPipelineAsync(System.Collections.Generic.IEnumerable<ViewModels.PluginCardViewModel> pluginQueue, IProgress<BuildPipelineProgress>? progress = null);
}
