using System;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

public interface IBuildTimerService
{
    event EventHandler<string>? ElapsedChanged;
    string CurrentElapsed { get; }
    TimeSpan RawElapsed { get; }
    void Start();
    void Stop();
    void Restart();
}
