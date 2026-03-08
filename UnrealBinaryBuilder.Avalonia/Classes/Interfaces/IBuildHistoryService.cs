using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

public interface IBuildHistoryService
{
    Task AddEntryAsync(BuildHistoryEntry entry);
    Task<List<BuildHistoryEntry>> GetHistoryAsync();
    Task DeleteEntryAsync(Guid id);
    Task ClearHistoryAsync();
    string SaveLogFile(Guid buildId, string logText);
    string GetLogFile(Guid buildId);
}
