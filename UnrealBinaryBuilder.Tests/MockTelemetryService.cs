using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using System.Collections.Generic;

namespace UnrealBinaryBuilder.Tests;

public class MockTelemetryService : ITelemetryService
{
    public List<string> TrackedEvents { get; } = new();
    public bool IsInitialized { get; private set; }
    public bool IsShutdown { get; private set; }

    public void Initialize(string productVersion) => IsInitialized = true;
    public void TrackEvent(string eventName) => TrackedEvents.Add(eventName);
    public void TrackProgressStart(string category, string step) => TrackedEvents.Add($"{category}::Start::{step}");
    public void TrackProgressEnd(string category, string step, bool isFailure = false) => TrackedEvents.Add($"{category}::End::{step}::Fail={isFailure}");
    public void TrackError(string message, TelemetrySeverity severity) => TrackedEvents.Add($"Error::{severity}::{message}");
    public void Shutdown() => IsShutdown = true;
}
