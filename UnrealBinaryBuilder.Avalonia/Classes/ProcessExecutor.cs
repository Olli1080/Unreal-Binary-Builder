using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnrealBinaryBuilder.Avalonia.Classes.Logging;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public interface IProcessExecutor
{
    Task<int> ExecuteAsync(string fileName, string arguments, string workingDirectory = "", LogCategory category = LogCategory.Build);
}

public class ProcessExecutor : IProcessExecutor
{
    private readonly IUBBLogger _logger;

    public ProcessExecutor(IUBBLogger logger)
    {
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(string fileName, string arguments, string workingDirectory = "", LogCategory category = LogCategory.Build)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (fileName.EndsWith(".bat", StringComparison.OrdinalIgnoreCase))
            {
                fileName = Path.ChangeExtension(fileName, ".sh");
            }

            if (File.Exists(fileName))
            {
                await EnsureExecutablePermissionAsync(fileName);
            }
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = startInfo };

        process.OutputDataReceived += (s, e) =>
        {
            if (e.Data != null) _logger.Log(e.Data, LogLevel.Info, category);
        };

        process.ErrorDataReceived += (s, e) =>
        {
            if (e.Data != null) _logger.Log(e.Data, LogLevel.Error, category);
        };

        try
        {
            if (!process.Start())
            {
                throw new Exception($"Failed to start process: {fileName}");
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Error starting process {fileName}: {ex.Message}", category);
            return -1;
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return process.ExitCode;
    }

    private async Task EnsureExecutablePermissionAsync(string fileName)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"+x \"{fileName}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            });

            if (process != null)
            {
                await process.WaitForExitAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to set executable permission on {fileName}: {ex.Message}", LogCategory.Build);
        }
    }
}
