using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

/// <summary>
/// Provides a service for running the initial setup chain for building Unreal Engine from source.
/// </summary>
public interface ISetupService
{
    /// <summary>
    /// Runs the entire setup chain asynchronously, which may include Setup.bat, GenerateProjectFiles.bat, and building the AutomationTool based on settings.
    /// </summary>
    /// <param name="enginePath">The root path to the Unreal Engine source code.</param>
    /// <param name="settings">The builder settings configuring the setup steps.</param>
    /// <param name="msBuild">The specific MSBuild executable to use for building tools, if required.</param>
    /// <param name="architecture">The target architecture to build tools for.</param>
    /// <returns>A task that represents the asynchronous setup operation. The task result is true if all requested steps succeeded, false otherwise.</returns>
    Task<bool> RunSetupChainAsync(string enginePath, BuilderSettingsJson settings, VisualStudioMsBuild? msBuild, string architecture);

    /// <summary>
    /// Runs the engine's Setup.bat script asynchronously with arguments prepared from settings to download dependencies.
    /// </summary>
    /// <param name="enginePath">The root path to the Unreal Engine source code.</param>
    /// <param name="settings">The builder settings configuring the setup script.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the exit code of the process.</returns>
    Task<int> RunSetupAsync(string enginePath, BuilderSettingsJson settings);

    /// <summary>
    /// Runs the engine's GenerateProjectFiles.bat script asynchronously to generate project files for the IDE.
    /// </summary>
    /// <param name="enginePath">The root path to the Unreal Engine source code.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the exit code of the process.</returns>
    Task<int> GenerateProjectFilesAsync(string enginePath);

    /// <summary>
    /// Builds the AutomationTool.sln project asynchronously using the specified MSBuild executable.
    /// </summary>
    /// <param name="enginePath">The root path to the Unreal Engine source code.</param>
    /// <param name="msBuild">The specific MSBuild executable to use.</param>
    /// <param name="architecture">The target architecture.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the exit code of the MSBuild process.</returns>
    Task<int> BuildAutomationToolAsync(string enginePath, VisualStudioMsBuild msBuild, string architecture);

    /// <summary>
    /// Prepares the command line arguments for Setup.bat based on the builder settings.
    /// </summary>
    /// <param name="settings">The builder settings configuring the setup process.</param>
    /// <returns>A formatted string of command line arguments for Setup.bat.</returns>
    string PrepareSetupArgs(BuilderSettingsJson settings);
}
