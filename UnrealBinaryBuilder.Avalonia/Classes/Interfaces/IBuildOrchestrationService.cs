using System.Collections.Generic;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

/// <summary>
/// Orchestrates the different stages of the Unreal Engine build process, allowing state to be saved, loaded, and resumed.
/// </summary>
public interface IBuildOrchestrationService
{
    /// <summary>
    /// Loads the saved build state if a previous build was interrupted or saved.
    /// </summary>
    /// <returns>The loaded <see cref="BuildState"/> if found, otherwise null.</returns>
    BuildState? LoadState();

    /// <summary>
    /// Saves the current build state to allow for resuming later.
    /// </summary>
    /// <param name="state">The build state to save.</param>
    void SaveState(BuildState state);

    /// <summary>
    /// Clears the currently saved build state.
    /// </summary>
    void ClearState();

    /// <summary>
    /// Marks a specific build stage as complete and saves the updated state.
    /// </summary>
    /// <param name="stage">The build stage that has been completed.</param>
    /// <param name="enginePath">The path to the Unreal Engine source.</param>
    /// <param name="gitHash">The Git commit hash associated with the build, if applicable.</param>
    /// <param name="settings">The current builder settings.</param>
    void MarkStageComplete(BuildStage stage, string enginePath, string? gitHash, BuilderSettingsJson settings);

    /// <summary>
    /// Determines whether a build can be resumed based on the provided engine path, Git hash, and settings.
    /// </summary>
    /// <param name="enginePath">The path to the Unreal Engine source.</param>
    /// <param name="gitHash">The Git commit hash associated with the build, if applicable.</param>
    /// <param name="settings">The current builder settings.</param>
    /// <returns>True if the build is resumable, otherwise false.</returns>
    bool IsResumable(string enginePath, string? gitHash, BuilderSettingsJson settings);
}
