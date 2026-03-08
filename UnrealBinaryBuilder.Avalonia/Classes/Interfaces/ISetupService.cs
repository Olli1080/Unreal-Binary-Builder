using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

public interface ISetupService
{
    /// <summary>
    /// Runs the setup chain (Setup.bat, GenerateProjectFiles.bat, Build AutomationTool) based on settings.
    /// </summary>
    /// <returns>True if all requested steps succeeded, false otherwise.</returns>
    Task<bool> RunSetupChainAsync(string enginePath, BuilderSettingsJson settings, VisualStudioMsBuild? msBuild, string architecture);

    /// <summary>
    /// Runs Setup.bat with arguments prepared from settings.
    /// </summary>
    Task<int> RunSetupAsync(string enginePath, BuilderSettingsJson settings);

    /// <summary>
    /// Runs GenerateProjectFiles.bat.
    /// </summary>
    Task<int> GenerateProjectFilesAsync(string enginePath);

    /// <summary>
    /// Builds AutomationTool.sln using the specified MSBuild.
    /// </summary>
    Task<int> BuildAutomationToolAsync(string enginePath, VisualStudioMsBuild msBuild, string architecture);

    /// <summary>
    /// Prepares the command line arguments for Setup.bat based on settings.
    /// </summary>
    string PrepareSetupArgs(BuilderSettingsJson settings);
}
