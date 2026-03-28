namespace UnrealBinaryBuilder.Avalonia.Models;

public class LanguageInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public LanguageInfo(string code, string name)
    {
        Code = code;
        Name = name;
    }
}
