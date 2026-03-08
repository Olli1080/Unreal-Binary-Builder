using System.Text.RegularExpressions;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

namespace UnrealBinaryBuilder.Avalonia.Classes;

public class LogFormatterService : ILogFormatterService
{
    private int _compiledFiles = 0;
    private int _compiledFilesTotal = 0;

    public void Reset()
    {
        _compiledFiles = 0;
        _compiledFilesTotal = 0;
    }

    public LogFormatResult FormatLogEntry(string message, bool isError)
    {
        var result = new LogFormatResult();
        if (string.IsNullOrEmpty(message)) return result;

        const string sp = @"\*{6} \[(\d+)\/(\d+)\]";
        const string pp = @"\w.+\.(cpp|cc|c|h|ispc)";

        if (Regex.IsMatch(message, sp))
        {
            var m = Regex.Match(message, sp);
            _compiledFiles = 0;
            if (int.TryParse(m.Groups[2].Value, out int t))
            {
                _compiledFilesTotal = t;
            }
        }

        if (Regex.IsMatch(message, pp))
        {
            _compiledFiles++;
            result.CompiledFilesText = $"[Compiled: {_compiledFiles}/{_compiledFilesTotal}]";
        }

        result.FormattedMessage = (isError ? "[ERROR] " : "") + message + "\n";
        return result;
    }
}
