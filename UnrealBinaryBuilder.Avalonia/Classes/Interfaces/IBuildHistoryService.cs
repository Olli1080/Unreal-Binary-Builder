using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

/// <summary>
/// Provides services for managing and accessing build history entries.
/// </summary>
public interface IBuildHistoryService
{
    /// <summary>
    /// Adds a new build history entry to the stored history asynchronously.
    /// </summary>
    /// <param name="entry">The build history entry to add.</param>
    /// <returns>A task that represents the asynchronous add operation.</returns>
    Task AddEntryAsync(BuildHistoryEntry entry);

    /// <summary>
    /// Retrieves the complete list of build history entries asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of build history entries.</returns>
    Task<List<BuildHistoryEntry>> GetHistoryAsync();

    /// <summary>
    /// Deletes a specific build history entry by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the build history entry to delete.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteEntryAsync(Guid id);

    /// <summary>
    /// Clears all build history entries asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous clear operation.</returns>
    Task ClearHistoryAsync();

    /// <summary>
    /// Saves the build log text to a file associated with the specified build ID.
    /// </summary>
    /// <param name="buildId">The unique identifier of the build.</param>
    /// <param name="logText">The log text to save.</param>
    /// <returns>The file path where the log was saved.</returns>
    string SaveLogFile(Guid buildId, string logText);

    /// <summary>
    /// Retrieves the log file content or path associated with the specified build ID.
    /// </summary>
    /// <param name="buildId">The unique identifier of the build.</param>
    /// <returns>The contents or path of the log file.</returns>
    string GetLogFile(Guid buildId);
}
