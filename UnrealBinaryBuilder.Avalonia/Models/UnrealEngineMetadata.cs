namespace UnrealBinaryBuilder.Avalonia.Models;

public record UnrealEngineMetadata(
    int Major,
    int Minor,
    int Patch,
    string VersionString,
    string FullVersionString,
    bool SupportWin32,
    bool SupportConsoles,
    bool SupportHTML5,
    bool SupportServerClientTargets,
    bool IsEngineSelection425OrAbove,
    bool SupportLinuxAArch64,
    bool SupportLinuxArm64,
    bool IsUE5
);
