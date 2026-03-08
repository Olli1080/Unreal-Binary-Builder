using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;

namespace UnrealBinaryBuilder.Tests;

public class MockPlatformService : IPlatformService
{
    public void OpenFolder(string path) { }
    public void OpenUrl(string url) { }
    public void ShutdownPC(int seconds) { }
}
