using System;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

namespace UnrealBinaryBuilder.Tests;

public class MockBuildTimerService : IBuildTimerService
{
    public event EventHandler<string>? ElapsedChanged;
    public string CurrentElapsed { get; set; } = "00:00:00";
    public TimeSpan RawElapsed { get; set; } = TimeSpan.Zero;

    public void Start() { }
    public void Stop() { }
    public void Restart() { }

    public void TriggerElapsedChanged(string elapsed)
    {
        CurrentElapsed = elapsed;
        ElapsedChanged?.Invoke(this, elapsed);
    }
}
