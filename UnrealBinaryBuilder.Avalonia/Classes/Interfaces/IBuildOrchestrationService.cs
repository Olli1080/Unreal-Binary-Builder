using System.Collections.Generic;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

public interface IBuildOrchestrationService
{
    BuildState? LoadState();
    void SaveState(BuildState state);
    void ClearState();
    void MarkStageComplete(BuildStage stage, string enginePath, string? gitHash, BuilderSettingsJson settings);
    bool IsResumable(string enginePath, string? gitHash, BuilderSettingsJson settings);
}
