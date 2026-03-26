using System;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

/// <summary>
/// Provides a service for tracking the elapsed time of an active build process.
/// </summary>
public interface IBuildTimerService
{
    /// <summary>
    /// Event triggered when the elapsed time string has changed.
    /// </summary>
    event EventHandler<string>? ElapsedChanged;

    /// <summary>
    /// Gets the formatted string representation of the currently elapsed build time.
    /// </summary>
    string CurrentElapsed { get; }

    /// <summary>
    /// Gets the raw elapsed time duration of the build.
    /// </summary>
    TimeSpan RawElapsed { get; }

    /// <summary>
    /// Starts the build timer.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the build timer.
    /// </summary>
    void Stop();

    /// <summary>
    /// Restarts the build timer from zero.
    /// </summary>
    void Restart();
}
