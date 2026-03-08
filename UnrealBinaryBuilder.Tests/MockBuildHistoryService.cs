using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Tests;

public class MockBuildHistoryService : IBuildHistoryService
{
    public List<BuildHistoryEntry> History { get; } = new();

    public Task AddEntryAsync(BuildHistoryEntry entry)
    {
        History.Insert(0, entry);
        return Task.CompletedTask;
    }

    public Task<List<BuildHistoryEntry>> GetHistoryAsync()
    {
        return Task.FromResult(new List<BuildHistoryEntry>(History));
    }

    public Task DeleteEntryAsync(Guid id)
    {
        var entry = History.Find(e => e.Id == id);
        if (entry != null) History.Remove(entry);
        return Task.CompletedTask;
    }

    public Task ClearHistoryAsync()
    {
        History.Clear();
        return Task.CompletedTask;
    }

    public string SaveLogFile(Guid buildId, string logText)
    {
        return $"{buildId}.log";
    }

    public string GetLogFile(Guid buildId)
    {
        return $"{buildId}.log";
    }
}
