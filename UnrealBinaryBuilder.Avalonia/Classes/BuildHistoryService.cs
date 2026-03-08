using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class BuildHistoryService : IBuildHistoryService
{
    private readonly string _historyFilePath;
    private readonly string _historyLogsPath;
    private List<BuildHistoryEntry>? _cachedHistory;

    public BuildHistoryService(string? baseSavedPath = null)
    {
        string root = baseSavedPath ?? SettingsService.DefaultSavedPath;
        _historyFilePath = Path.Combine(root, "Saved", "History.json");
        _historyLogsPath = Path.Combine(root, "HistoryLogs");
        
        if (!Directory.Exists(Path.GetDirectoryName(_historyFilePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_historyFilePath)!);
        }
        
        if (!Directory.Exists(_historyLogsPath))
        {
            Directory.CreateDirectory(_historyLogsPath);
        }
    }

    public async Task AddEntryAsync(BuildHistoryEntry entry)
    {
        var history = await GetHistoryAsync();
        history.Insert(0, entry);
        _cachedHistory = history;
        await SaveHistoryAsync(history);
    }

    public async Task<List<BuildHistoryEntry>> GetHistoryAsync()
    {
        if (_cachedHistory != null) return _cachedHistory;

        if (File.Exists(_historyFilePath))
        {
            try
            {
                string json = await File.ReadAllTextAsync(_historyFilePath);
                _cachedHistory = JsonConvert.DeserializeObject<List<BuildHistoryEntry>>(json, [new Newtonsoft.Json.Converters.StringEnumConverter()]) ?? new List<BuildHistoryEntry>();
                return _cachedHistory;
            }
            catch
            {
                return new List<BuildHistoryEntry>();
            }
        }
        
        return new List<BuildHistoryEntry>();
    }

    public async Task DeleteEntryAsync(Guid id)
    {
        var history = await GetHistoryAsync();
        var entryToRemove = history.FirstOrDefault(e => e.Id == id);
        if (entryToRemove != null)
        {
            history.Remove(entryToRemove);
            _cachedHistory = history;
            
            // Also delete the log file if it exists
            string logFile = GetLogFile(id);
            if (File.Exists(logFile))
            {
                try { File.Delete(logFile); } catch { }
            }
            
            await SaveHistoryAsync(history);
        }
    }

    public async Task ClearHistoryAsync()
    {
        _cachedHistory = new List<BuildHistoryEntry>();
        await SaveHistoryAsync(_cachedHistory);
        
        // Also clear logs
        if (Directory.Exists(_historyLogsPath))
        {
            foreach (var file in Directory.GetFiles(_historyLogsPath))
            {
                try { File.Delete(file); } catch { }
            }
        }
    }

    public string SaveLogFile(Guid buildId, string logText)
    {
        string filePath = GetLogFile(buildId);
        File.WriteAllText(filePath, logText);
        return filePath;
    }

    public string GetLogFile(Guid buildId)
    {
        return Path.Combine(_historyLogsPath, $"{buildId}.log");
    }

    private async Task SaveHistoryAsync(List<BuildHistoryEntry> history)
    {
        string json = JsonConvert.SerializeObject(history, Formatting.Indented, [new Newtonsoft.Json.Converters.StringEnumConverter()]);
        await File.WriteAllTextAsync(_historyFilePath, json);
    }
}
