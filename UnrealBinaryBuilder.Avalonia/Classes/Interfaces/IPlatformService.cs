namespace UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

public interface IPlatformService
{
    void OpenFolder(string path);
    void OpenUrl(string url);
    void ShutdownPC(int seconds);
}
