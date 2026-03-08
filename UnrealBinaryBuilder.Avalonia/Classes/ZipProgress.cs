namespace UnrealBinaryBuilder.Avalonia.Classes;

public class ZipProgress
{
    public string Message { get; set; } = string.Empty;
    public double Progress { get; set; }
    public string State { get; set; } = string.Empty;
    public string CurrentFile { get; set; } = string.Empty;
    public string TotalResult { get; set; } = string.Empty;
}
