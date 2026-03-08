using System.Collections.Generic;
using System.Text;

namespace UnrealBinaryBuilder.Avalonia.Models;

/// <summary>
/// Represents arguments for a BuildGraph command.
/// </summary>
public class BuildGraphArguments
{
    public string Target { get; set; } = "Make Installed Build Win64";
    public string Script { get; set; } = "";
    public Dictionary<string, string> SetFlags { get; set; } = new();
    public bool Clean { get; set; }
    public string CustomOptions { get; set; } = "";

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("BuildGraph");
        
        if (!string.IsNullOrEmpty(Target))
        {
            sb.Append($" -target=\"{Target}\"");
        }

        if (!string.IsNullOrEmpty(Script))
        {
            sb.Append($" -script=\"{Script}\"");
        }

        foreach (var flag in SetFlags)
        {
            string value = flag.Value;
            if (value.Contains(" "))
            {
                value = $"\"{value}\"";
            }
            sb.Append($" -set:{flag.Key}={value}");
        }

        if (Clean)
        {
            sb.Append(" -Clean");
        }

        if (!string.IsNullOrEmpty(CustomOptions))
        {
            sb.Append($" {CustomOptions}");
        }

        return sb.ToString().Trim();
    }
}

/// <summary>
/// Represents arguments for a UAT command (e.g., BuildPlugin).
/// </summary>
public class UatArguments
{
    public string Command { get; set; } = "BuildPlugin";
    public Dictionary<string, string> Arguments { get; set; } = new();
    public List<string> Flags { get; set; } = new();

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Command);

        foreach (var arg in Arguments)
        {
            if (arg.Value.Contains(" ") || arg.Value.Contains(";"))
            {
                sb.Append($" -{arg.Key}=\"{arg.Value}\"");
            }
            else
            {
                sb.Append($" -{arg.Key}={arg.Value}");
            }
        }

        foreach (var flag in Flags)
        {
            sb.Append($" -{flag}");
        }

        return sb.ToString().Trim();
    }
}
