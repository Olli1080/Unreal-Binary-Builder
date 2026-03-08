using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Tests;

public class MockSetupService : ISetupService
{
    public bool RunSetupChainAsyncResult { get; set; } = true;
    public int RunSetupAsyncResult { get; set; } = 0;
    public int GenerateProjectFilesAsyncResult { get; set; } = 0;
    public int BuildAutomationToolAsyncResult { get; set; } = 0;

    public Task<bool> RunSetupChainAsync(string enginePath, BuilderSettingsJson settings, VisualStudioMsBuild? msBuild, string architecture)
    {
        return Task.FromResult(RunSetupChainAsyncResult);
    }

    public Task<int> RunSetupAsync(string enginePath, BuilderSettingsJson settings)
    {
        return Task.FromResult(RunSetupAsyncResult);
    }

    public Task<int> GenerateProjectFilesAsync(string enginePath)
    {
        return Task.FromResult(GenerateProjectFilesAsyncResult);
    }

    public Task<int> BuildAutomationToolAsync(string enginePath, VisualStudioMsBuild msBuild, string architecture)
    {
        return Task.FromResult(BuildAutomationToolAsyncResult);
    }

    public string PrepareSetupArgs(BuilderSettingsJson settings)
    {
        return "--mock-args";
    }
}
