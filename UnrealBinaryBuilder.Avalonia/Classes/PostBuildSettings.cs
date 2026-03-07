using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class ZipProgress
{
    public string Message { get; set; } = string.Empty;
    public double Progress { get; set; }
    public string State { get; set; } = string.Empty;
    public string CurrentFile { get; set; } = string.Empty;
    public string TotalResult { get; set; } = string.Empty;
}

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

    public async Task SavePluginToZip(string sourcePath, string zipLocationToSave, bool bZipForMarketplace, bool bFastCompression, IProgress<ZipProgress>? progress = null)
    {
        CompressionLevel cl = bFastCompression ? CompressionLevel.Fastest : CompressionLevel.SmallestSize;
        
        await Task.Run(() =>
        {
            using FileStream output = new FileStream(zipLocationToSave, FileMode.Create);
            using (var zipFile = new ZipArchive(output, ZipArchiveMode.Create))
            {
                IEnumerable<string> files = Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories);
                List<string> filesToAdd = [];

                foreach (string file in files)
                {
                    string currentFilePath = Path.GetFullPath(file).ToLower();
                    if (bZipForMarketplace && (currentFilePath.Contains(@"\binaries\") || currentFilePath.Contains(@"\intermediate\") ||
                                               currentFilePath.Contains(@"/binaries/") || currentFilePath.Contains(@"/intermediate/")))
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

                    string entry = Path.GetDirectoryName(file)!.Replace(sourcePath, string.Empty);
                    entry = Path.Combine(entry, Path.GetFileName(file));
                    entry = entry.TrimStart(Path.DirectorySeparatorChar).TrimStart(Path.AltDirectorySeparatorChar);

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
        CompressionLevel cl = settings.bZipEngineFastCompression ? CompressionLevel.Fastest : CompressionLevel.SmallestSize;

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
                string currentFilePath = Path.GetFullPath(file).ToLower();
                string extension = Path.GetExtension(file).ToLower();

                if (!settings.bZipEnginePDB && extension == ".pdb") bSkipFile = true;
                if (!settings.bZipEngineDebug && extension == ".debug") bSkipFile = true;
                if (!settings.bZipEngineDocumentation && !currentFilePath.Contains(@"\source\") && !currentFilePath.Contains(@"/source/") && (currentFilePath.Contains(@"\documentation\") || currentFilePath.Contains(@"/documentation/"))) bSkipFile = true;
                if (!settings.bZipEngineExtras && !currentFilePath.Contains(@"\extras\redist\") && !currentFilePath.Contains(@"/extras/redist/") && (currentFilePath.Contains(@"\extras\") || currentFilePath.Contains(@"/extras/"))) bSkipFile = true;
                
                if (!settings.bZipEngineSource)
                {
                    if (currentFilePath.Contains(@"\source\developer\") || currentFilePath.Contains(@"/source/developer/")) bSkipFile = true;
                    else if (currentFilePath.Contains(@"\source\editor\") || currentFilePath.Contains(@"/source/editor/")) bSkipFile = true;
                    else if (currentFilePath.Contains(@"\source\programs\") || currentFilePath.Contains(@"/source/programs/")) bSkipFile = true;
                    else if (currentFilePath.Contains(@"\source\runtime\") || currentFilePath.Contains(@"/source/runtime/")) bSkipFile = true;
                    else if (currentFilePath.Contains(@"\source\thirdparty\") || currentFilePath.Contains(@"/source/thirdparty/")) bSkipFile = true;
                }

                if (!settings.bZipEngineFeaturePacks && (currentFilePath.Contains(@"\featurepacks\") || currentFilePath.Contains(@"/featurepacks/"))) bSkipFile = true;
                if (!settings.bZipEngineSamples && (currentFilePath.Contains(@"\samples\") || currentFilePath.Contains(@"/samples/"))) bSkipFile = true;
                if (!settings.bZipEngineTemplates && !currentFilePath.Contains(@"\source\") && !currentFilePath.Contains(@"/source/") && !currentFilePath.Contains(@"\content\editor") && !currentFilePath.Contains(@"/content/editor") && (currentFilePath.Contains(@"\templates\") || currentFilePath.Contains(@"/templates/"))) bSkipFile = true;

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
