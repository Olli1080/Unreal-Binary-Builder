using System.Threading.Tasks;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

/// <summary>
/// Service responsible for the initial application startup and configuration.
/// </summary>
public interface IAppInitializer
{
    /// <summary>
    /// Performs all necessary startup tasks such as initializing telemetry, 
    /// loading settings, checking for updates, and applying themes.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization process.</returns>
    Task InitializeAsync();
}
