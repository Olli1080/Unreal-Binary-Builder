using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class PostBuildSettings
{
    private static CancellationTokenSource _zipCancelTokenSource = new();
    private CancellationToken _zipCancelToken = _zipCancelTokenSource.Token;

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

    public async Task SavePluginToZip(string sourcePath, string zipLocationToSave, bool zipForMarketplace, bool fastCompression, IProgress<ZipProgress>? progress = null)
    {
        CompressionLevel cl = fastCompression ? CompressionLevel.Fastest : CompressionLevel.SmallestSize;
        sourcePath = PathHelpers.NormalizePath(sourcePath);
        
        await Task.Run(() =>
        {
            using FileStream output = new FileStream(zipLocationToSave, FileMode.Create);
            using (var zipFile = new ZipArchive(output, ZipArchiveMode.Create))
            {
                IEnumerable<string> files = Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories);
                List<string> filesToAdd = [];

                foreach (string file in files)
                {
                    string currentFilePath = PathHelpers.ToUnixPath(Path.GetFullPath(file)).ToLower();
                    if (zipForMarketplace && (currentFilePath.Contains("/binaries/") || currentFilePath.Contains("/intermediate/")))
                    {
                        continue;
                    }
                    filesToAdd.Add(file);
                }

                int entriesSaved = 0;
                int totalFiles = filesToAdd.Count;

                foreach (string file in filesToAdd)
                {
                    _zipCancelToken.ThrowIfCancellationRequested();

                    string normalizedFile = PathHelpers.ToUnixPath(file);
                    string entry = normalizedFile.Replace(sourcePath, string.Empty).TrimStart('/');

                    zipFile.CreateEntryFromFile(file, entry, cl);
                    ++entriesSaved;

                    progress?.Report(new ZipProgress
                    {
                        Progress = (double)entriesSaved / totalFiles * 100,
                        CurrentFile = Path.GetFileName(file),
                        Message = $"Saving: {entriesSaved}/{totalFiles}"
                    });
                }
            }
        }, _zipCancelToken);
    }

    public async Task SaveToZip(string inBuildDirectory, string zipLocationToSave, BuilderSettingsJson settings, IProgress<ZipProgress>? progress = null)
    {
        CompressionLevel cl = settings.ZipEngineFastCompression ? CompressionLevel.Fastest : CompressionLevel.SmallestSize;

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
            long totalSizeToZip = 0;
            long skippedSize = 0;
            int skippedFiles = 0;
            int addedFiles = 0;

            foreach (string file in files)
            {
                _zipCancelToken.ThrowIfCancellationRequested();
                bool bSkipFile = false;
                string currentFilePath = PathHelpers.ToUnixPath(Path.GetFullPath(file)).ToLower();
                string extension = Path.GetExtension(file).ToLower();

                if (!settings.ZipEnginePDB && extension == ".pdb") bSkipFile = true;
                if (!settings.ZipEngineDebug && extension == ".debug") bSkipFile = true;
                if (!settings.ZipEngineDocumentation && !currentFilePath.Contains("/source/") && currentFilePath.Contains("/documentation/")) bSkipFile = true;
                if (!settings.ZipEngineExtras && !currentFilePath.Contains("/extras/redist/") && currentFilePath.Contains("/extras/")) bSkipFile = true;
                
                if (!settings.ZipEngineSource)
                {
                    if (currentFilePath.Contains("/source/developer/")) bSkipFile = true;
                    else if (currentFilePath.Contains("/source/editor/")) bSkipFile = true;
                    else if (currentFilePath.Contains("/source/programs/")) bSkipFile = true;
                    else if (currentFilePath.Contains("/source/runtime/")) bSkipFile = true;
                    else if (currentFilePath.Contains("/source/thirdparty/")) bSkipFile = true;
                }

                if (!settings.ZipEngineFeaturePacks && currentFilePath.Contains("/featurepacks/")) bSkipFile = true;
                if (!settings.ZipEngineSamples && currentFilePath.Contains("/samples/")) bSkipFile = true;
                if (!settings.ZipEngineTemplates && !currentFilePath.Contains("/source/") && !currentFilePath.Contains("/content/editor") && currentFilePath.Contains("/templates/")) bSkipFile = true;

                long fileSize = new FileInfo(file).Length;
                totalSize += fileSize;

                if (bSkipFile)
                {
                    skippedFiles++;
                    skippedSize += fileSize;
                }
                else
                {
                    filesToAdd.Add(file);
                    addedFiles++;
                    totalSizeToZip += fileSize;
                }

                progress?.Report(new ZipProgress
                {
                    Message = $"Total: {files.Count()}. Added: {addedFiles}. Skipped: {skippedFiles}",
                    TotalResult = $"Total Size: {BytesToString(totalSize)}. To Zip: {BytesToString(totalSizeToZip)}. Skipped: {BytesToString(skippedSize)}"
                });
            }

            int entriesSaved = 0;
            long processedSize = 0;
            int totalFilesToAdd = filesToAdd.Count;

            foreach (string file in filesToAdd)
            {
                _zipCancelToken.ThrowIfCancellationRequested();

                string entryName = Path.GetRelativePath(inBuildDirectory, file);
                zipFile.CreateEntryFromFile(file, entryName, cl);
                
                entriesSaved++;
                processedSize += new FileInfo(file).Length;

                progress?.Report(new ZipProgress
                {
                    State = "Saving zip file...",
                    CurrentFile = Path.GetFileName(file),
                    Progress = (double)entriesSaved / totalFilesToAdd * 100,
                    Message = $"Saving: {entriesSaved}/{totalFilesToAdd}",
                    TotalResult = $"Total Size: {BytesToString(totalSize)}. To Zip: {BytesToString(totalSizeToZip)}. Skipped: {BytesToString(skippedSize)}. Processed: {BytesToString(processedSize)}"
                });
            }

        }, _zipCancelToken);
    }

    public void CancelTask()
    {
        _zipCancelTokenSource.Cancel();
    }

    public static string BytesToString(long byteCount)
    {
        string[] suf = ["B", "KB", "MB", "GB", "TB"];
        if (byteCount == 0) return "0" + suf[0];
        long bytes = Math.Abs(byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign(byteCount) * num).ToString() + suf[place];
    }
}
