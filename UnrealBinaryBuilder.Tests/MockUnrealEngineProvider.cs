using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Tests;

public class MockUnrealEngineProvider : IUnrealEngineProvider
{
    public UnrealEngineMetadata? Metadata { get; set; } = new UnrealEngineMetadata(5, 0, 0, "5.0", "5.0.0", false, false, false, true, true, false, true, true, true, true);
    public string AutomationPath { get; set; } = "AutomationTool.exe";

    public string GetAutomationPath(string enginePath, bool isUE5) => AutomationPath;

    public UnrealEngineMetadata? GetEngineMetadata(string enginePath) => Metadata;
}
