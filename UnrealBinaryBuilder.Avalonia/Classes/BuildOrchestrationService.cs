using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class BuildOrchestrationService : IBuildOrchestrationService
{
    private readonly string _stateFilePath;

    public BuildOrchestrationService(string? baseSavedPath = null)
    {
        string root = baseSavedPath ?? SettingsService.DefaultSavedPath;
        _stateFilePath = Path.Combine(root, "Saved", "CurrentBuildState.json");
        
        if (!Directory.Exists(Path.GetDirectoryName(_stateFilePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_stateFilePath)!);
        }
    }

    public BuildState? LoadState()
    {
        if (File.Exists(_stateFilePath))
        {
            try
            {
                string json = File.ReadAllText(_stateFilePath);
                return JsonConvert.DeserializeObject<BuildState>(json, [new Newtonsoft.Json.Converters.StringEnumConverter()]);
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    public void SaveState(BuildState state)
    {
        state.LastUpdate = DateTime.Now;
        string json = JsonConvert.SerializeObject(state, Formatting.Indented, [new Newtonsoft.Json.Converters.StringEnumConverter()]);
        File.WriteAllText(_stateFilePath, json);
    }

    public void ClearState()
    {
        if (File.Exists(_stateFilePath))
        {
            try { File.Delete(_stateFilePath); } catch { }
        }
    }

    public void MarkStageComplete(BuildStage stage, string enginePath, string? gitHash, BuilderSettingsJson settings)
    {
        var state = LoadState() ?? new BuildState
        {
            EnginePath = enginePath,
            GitHash = gitHash,
            Settings = settings
        };

        if (!state.CompletedStages.Contains(stage))
        {
            state.CompletedStages.Add(stage);
        }
        
        // Update hash/path/settings in case they changed (though IsResumable should prevent mismatch)
        state.EnginePath = enginePath;
        state.GitHash = gitHash;
        state.Settings = settings;

        SaveState(state);
    }

    public bool IsResumable(string enginePath, string? gitHash, BuilderSettingsJson settings)
    {
        var state = LoadState();
        if (state == null) return false;

        // Verify it's the same engine, same git hash, and same settings
        if (state.EnginePath != enginePath) return false;
        if (state.GitHash != gitHash) return false;
        
        // Simple JSON comparison for settings
        string currentSettingsJson = JsonConvert.SerializeObject(settings);
        string stateSettingsJson = JsonConvert.SerializeObject(state.Settings);
        
        return currentSettingsJson == stateSettingsJson && state.CompletedStages.Any();
    }
}
