using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public interface IProcessExecutor
{
    Task<int> ExecuteAsync(string fileName, string arguments, string workingDirectory = "", Action<string>? onOutput = null, Action<string>? onError = null);
}

public class ProcessExecutor : IProcessExecutor
{
    public async Task<int> ExecuteAsync(string fileName, string arguments, string workingDirectory = "", Action<string>? onOutput = null, Action<string>? onError = null)
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
            if (e.Data != null) onOutput?.Invoke(e.Data);
        };

        process.ErrorDataReceived += (s, e) =>
        {
            if (e.Data != null) onError?.Invoke(e.Data);
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
