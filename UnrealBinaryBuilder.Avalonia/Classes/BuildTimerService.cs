using System;
using System.Diagnostics;
using Avalonia.Threading;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class BuildTimerService : IBuildTimerService
{
    private readonly Stopwatch _buildStopwatch = new();
    private readonly DispatcherTimer _buildTimer = new(DispatcherPriority.Background);

    public event EventHandler<string>? ElapsedChanged;

    public string CurrentElapsed => _buildStopwatch.Elapsed.ToString(@"hh\:mm\:ss");
    public TimeSpan RawElapsed => _buildStopwatch.Elapsed;

    public BuildTimerService()
    {
        _buildTimer.Interval = TimeSpan.FromSeconds(1);
        _buildTimer.Tick += (s, e) => ElapsedChanged?.Invoke(this, CurrentElapsed);
    }

    public void Start()
    {
        _buildTimer.Start();
    }

    public void Stop()
    {
        _buildStopwatch.Stop();
        _buildTimer.Stop();
    }

    public void Restart()
    {
        _buildStopwatch.Restart();
        _buildTimer.Start();
    }
}
