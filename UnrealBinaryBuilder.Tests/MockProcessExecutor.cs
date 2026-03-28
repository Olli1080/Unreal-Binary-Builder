using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Classes.Logging;
using System.Threading.Tasks;

namespace UnrealBinaryBuilder.Tests;

public class MockProcessExecutor : IProcessExecutor
{
    public int ExitCode { get; set; } = 0;
    public string? LastCommand { get; private set; }
    public string? LastArgs { get; private set; }

    public Task<int> ExecuteAsync(string fileName, string arguments, string workingDirectory = "", LogCategory category = LogCategory.Build)
    {
        LastCommand = fileName;
        LastArgs = arguments;
        return Task.FromResult(ExitCode);
    }
}
