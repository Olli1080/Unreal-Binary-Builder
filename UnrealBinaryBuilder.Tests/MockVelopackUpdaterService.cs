using UnrealBinaryBuilder.Avalonia.Classes;
using System.Threading.Tasks;

namespace UnrealBinaryBuilder.Tests;

public class MockVelopackUpdaterService : IVelopackUpdaterService
{
    public bool IsUpdateAvailable { get; set; } = false;

    public Task CheckForUpdatesAsync(bool silent = false) => Task.CompletedTask;

    public Task DownloadUpdatesAsync() => Task.CompletedTask;

    public void ApplyUpdatesAndRestart() { }
}
