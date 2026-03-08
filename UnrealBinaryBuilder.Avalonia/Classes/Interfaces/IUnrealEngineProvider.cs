using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

public interface IUnrealEngineProvider
{
    UnrealEngineMetadata? GetEngineMetadata(string enginePath);
    string GetAutomationPath(string enginePath, bool isUE5);
}
