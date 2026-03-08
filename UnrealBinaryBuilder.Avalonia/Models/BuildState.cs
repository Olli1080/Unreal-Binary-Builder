using System;
using System.Collections.Generic;

namespace UnrealBinaryBuilder.Avalonia.Models;

public class BuildState
{
    public string EnginePath { get; set; } = string.Empty;
    public string? GitHash { get; set; }
    public BuilderSettingsJson? Settings { get; set; }
    public List<BuildStage> CompletedStages { get; set; } = new();
    public DateTime LastUpdate { get; set; }
}
