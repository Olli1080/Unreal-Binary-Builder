using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Tests;

public class MockEngineBuildService : IEngineBuildService
{
    public bool BuildEngineAsyncResult { get; set; } = true;
    public string PrepareEngineCommandlineResult { get; set; } = "mock-args";

    public Task<bool> BuildEngineAsync(string enginePath, BuilderSettingsJson settings, VisualStudioVersion? vsVersion, UnrealEngineMetadata? metadata)
    {
        return Task.FromResult(BuildEngineAsyncResult);
    }

    public string PrepareEngineCommandline(BuilderSettingsJson settings, UnrealEngineMetadata? metadata, VisualStudioVersion? vsVersion)
    {
        return PrepareEngineCommandlineResult;
    }
}
