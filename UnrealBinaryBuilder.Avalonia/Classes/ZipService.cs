using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class ZipService : IZipService
{
    private CancellationTokenSource _zipCancelTokenSource = new();
    private CancellationToken _zipCancelToken;

    public ZipService()
    {
        _zipCancelToken = _zipCancelTokenSource.Token;
    }

    public bool CanSaveToZip(string zipPath)
    {
        string? directoryName = Path.GetDirectoryName(zipPath);
        return !string.IsNullOrEmpty(zipPath) && directoryName != null && DirectoryIsWritable(directoryName);
    }

    public bool DirectoryIsWritable(string directoryPath)
    {
        directoryPath = PathHelpers.NormalizePath(directoryPath);
        if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
        {
            return false;
        }

        try
        {
            string testFile = Path.Combine(directoryPath, Path.GetRandomFileName());
            using (FileStream fs = File.Create(testFile, 1, FileOptions.DeleteOnClose))
            {
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    public void PrepareToSave()
    {
        _zipCancelTokenSource.Dispose();
        _zipCancelTokenSource = new CancellationTokenSource();
        _zipCancelToken = _zipCancelTokenSource.Token;
    }

    public void CancelTask()
    {
        _zipCancelTokenSource.Cancel();
    }

    public async Task SavePluginToZip(string sourcePath, string zipLocationToSave, bool zipForMarketplace, bool fastCompression, IProgress<ZipProgress>? progress = null)
    {
        CompressionLevel cl = fastCompression ? CompressionLevel.Fastest : CompressionLevel.SmallestSize;
        sourcePath = PathHelpers.NormalizePath(sourcePath);

        await Task.Run(() =>
        {
            if (File.Exists(zipLocationToSave))
            {
                File.Delete(zipLocationToSave);
            }

            using FileStream output = new FileStream(zipLocationToSave, FileMode.CreateNew);
            using var zipFile = new ZipArchive(output, ZipArchiveMode.Create);

            progress?.Report(new ZipProgress { State = "Preparing files..." });
            
            IEnumerable<string> files = Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories).ToArray();
            _zipCancelToken.ThrowIfCancellationRequested();

            List<string> filesToAdd = [];
            foreach (var file in files)
            {
                string relativePath = Path.GetRelativePath(sourcePath, file).Replace('\\', '/');
                if (zipForMarketplace && (relativePath.StartsWith("Binaries/") || relativePath.StartsWith("Intermediate/")))
                {
                    continue;
                }
                filesToAdd.Add(file);
            }

            long totalSize = 0;
            foreach (var file in filesToAdd)
            {
                totalSize += new FileInfo(file).Length;
            }

            long currentSize = 0;
            int fileCount = 0;
            foreach (var file in filesToAdd)
            {
                _zipCancelToken.ThrowIfCancellationRequested();
                string relativePath = Path.GetRelativePath(sourcePath, file).Replace('\\', '/');
                
                FileInfo fileInfo = new FileInfo(file);
                zipFile.CreateEntryFromFile(file, relativePath, cl);
                
                currentSize += fileInfo.Length;
                fileCount++;
                
                progress?.Report(new ZipProgress 
                { 
                    State = $"Zipping {fileCount}/{filesToAdd.Count}...",
                    Progress = (double)currentSize / totalSize * 100,
                    CurrentFile = relativePath
                });
            }
        }, _zipCancelToken);
    }

    public async Task SaveToZip(string inBuildDirectory, string zipLocationToSave, BuilderSettingsJson settings, IProgress<ZipProgress>? progress = null)
    {
        CompressionLevel cl = settings.ZipEngineFastCompression ? CompressionLevel.Fastest : CompressionLevel.SmallestSize;
        inBuildDirectory = PathHelpers.NormalizePath(inBuildDirectory);

        await Task.Run(() =>
        {
            if (File.Exists(zipLocationToSave))
            {
                File.Delete(zipLocationToSave);
            }

            using FileStream output = new FileStream(zipLocationToSave, FileMode.CreateNew);
            using var zipFile = new ZipArchive(output, ZipArchiveMode.Create);

            progress?.Report(new ZipProgress { State = "Preparing files..." });
            
            IEnumerable<string> files = Directory.EnumerateFiles(inBuildDirectory, "*.*", SearchOption.AllDirectories).ToArray();
            _zipCancelToken.ThrowIfCancellationRequested();

            List<string> filesToAdd = [];
            long totalSize = 0;

            foreach (string currentFilePath in files)
            {
                _zipCancelToken.ThrowIfCancellationRequested();
                bool bSkipFile = false;

                if (!settings.ZipEngineSource && currentFilePath.Contains("/Source/")) bSkipFile = true;
                if (!settings.ZipEngineExtras && currentFilePath.Contains("/Extras/")) bSkipFile = true;
                if (!settings.ZipEngineSamples && currentFilePath.Contains("/Samples/")) bSkipFile = true;
                if (!settings.ZipEngineTemplates && currentFilePath.Contains("/Templates/")) bSkipFile = true;
                if (!settings.ZipEngineDocumentation && currentFilePath.Contains("/Documentation/")) bSkipFile = true;
                if (!settings.ZipEngineFeaturePacks && currentFilePath.Contains("/FeaturePacks/")) bSkipFile = true;

                if (!settings.ZipEnginePDB && currentFilePath.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase)) bSkipFile = true;
                if (!settings.ZipEngineDebug && currentFilePath.EndsWith(".debug", StringComparison.OrdinalIgnoreCase)) bSkipFile = true;

                if (!bSkipFile)
                {
                    filesToAdd.Add(currentFilePath);
                    totalSize += new FileInfo(currentFilePath).Length;
                }
            }

            long currentSize = 0;
            int fileCount = 0;
            foreach (var file in filesToAdd)
            {
                _zipCancelToken.ThrowIfCancellationRequested();
                string relativePath = Path.GetRelativePath(inBuildDirectory, file).Replace('\\', '/');
                
                FileInfo fileInfo = new FileInfo(file);
                zipFile.CreateEntryFromFile(file, relativePath, cl);
                
                currentSize += fileInfo.Length;
                fileCount++;
                
                progress?.Report(new ZipProgress 
                { 
                    State = $"Zipping {fileCount}/{filesToAdd.Count}...",
                    Progress = (double)currentSize / totalSize * 100,
                    CurrentFile = relativePath
                });
            }
        }, _zipCancelToken);
    }
}
