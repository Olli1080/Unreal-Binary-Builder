using Newtonsoft.Json;

namespace UnrealBinaryBuilder.Avalonia.Models;

/// <summary>
/// Represents a saved snapshot of builder settings.
/// </summary>
public class BuildPreset
{
    public BuildPreset(string name, BuilderSettingsJson settings)
    {
        Name = name;
        Settings = settings;
    }

    /// <summary>
    /// The display name of the preset.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The settings associated with this preset.
    /// </summary>
    public BuilderSettingsJson Settings { get; set; }
}
