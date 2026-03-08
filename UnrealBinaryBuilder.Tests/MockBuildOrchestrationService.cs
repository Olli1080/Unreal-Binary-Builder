using System.Collections.Generic;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Tests;

public class MockBuildOrchestrationService : IBuildOrchestrationService
{
    public BuildState? CurrentState { get; set; }

    public BuildState? LoadState() => CurrentState;
    
    public void SaveState(BuildState state) => CurrentState = state;
    
    public void ClearState() => CurrentState = null;

    public void MarkStageComplete(BuildStage stage, string enginePath, string? gitHash, BuilderSettingsJson settings)
    {
        CurrentState ??= new BuildState { EnginePath = enginePath, GitHash = gitHash, Settings = settings };
        if (!CurrentState.CompletedStages.Contains(stage)) CurrentState.CompletedStages.Add(stage);
    }

    public bool IsResumable(string enginePath, string? gitHash, BuilderSettingsJson settings)
    {
        if (CurrentState == null) return false;
        return CurrentState.EnginePath == enginePath;
    }
}
