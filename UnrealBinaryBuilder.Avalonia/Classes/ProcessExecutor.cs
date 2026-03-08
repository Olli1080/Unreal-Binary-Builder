using System;
using System.Diagnostics;
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

        if (!process.Start())
        {
            throw new Exception($"Failed to start process: {fileName}");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return process.ExitCode;
    }
}
