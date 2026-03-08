using System;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

public interface IZipService
{
    bool CanSaveToZip(string zipPath);
    void PrepareToSave();
    void CancelTask();
    Task SavePluginToZip(string sourcePath, string zipLocationToSave, bool bZipForMarketplace, bool bFastCompression, IProgress<ZipProgress>? progress = null);
    Task SaveToZip(string inBuildDirectory, string zipLocationToSave, BuilderSettingsJson settings, IProgress<ZipProgress>? progress = null);
}
