using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Tests;

public class MockZipService : IZipService
{
    public bool CanSaveToZipResult { get; set; } = true;

    public bool CanSaveToZip(string zipPath) => CanSaveToZipResult;

    public void PrepareToSave() { }

    public void CancelTask() { }

    public Task SavePluginToZip(string sourcePath, string zipLocationToSave, bool zipForMarketplace, bool fastCompression, IProgress<ZipProgress>? progress = null)
    {
        return Task.CompletedTask;
    }

    public Task SaveToZip(string inBuildDirectory, string zipLocationToSave, BuilderSettingsJson settings, IProgress<ZipProgress>? progress = null)
    {
        return Task.CompletedTask;
    }
}
