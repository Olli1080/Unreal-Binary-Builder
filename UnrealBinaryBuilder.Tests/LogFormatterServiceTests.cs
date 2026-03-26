using UnrealBinaryBuilder.Avalonia.Classes;

namespace UnrealBinaryBuilder.Tests;

public class LogFormatterServiceTests
{
    private readonly LogFormatterService _formatter;

    public LogFormatterServiceTests()
    {
        _formatter = new LogFormatterService();
    }

    [Fact]
    public void FormatLogEntry_DetectsTotalFiles()
    {
        // Arrange
        string log = "****** [1/100]";

        // Act
        var result = _formatter.FormatLogEntry(log, false);

        // Assert
        // We don't see Total in result, but we see compiled 1/100 if a file follows
        var fileResult = _formatter.FormatLogEntry("File.cpp", false);
        Assert.Equal("[Compiled: 1/100]", fileResult.CompiledFilesText);
    }

    [Fact]
    public void FormatLogEntry_IncrementsCompiledFiles()
    {
        // Arrange
        _formatter.FormatLogEntry("****** [1/10]", false);
        _formatter.FormatLogEntry("File1.cpp", false);

        // Act
        var result = _formatter.FormatLogEntry("File2.cpp", false);

        // Assert
        Assert.Equal("[Compiled: 2/10]", result.CompiledFilesText);
    }

    [Fact]
    public void FormatLogEntry_HandlesErrorPrefix()
    {
        // Act
        var result = _formatter.FormatLogEntry("Something went wrong", true);

        // Assert
        Assert.StartsWith("[ERROR]", result.FormattedMessage);
    }

    [Fact]
    public void Reset_ClearsCounters()
    {
        // Arrange
        _formatter.FormatLogEntry("****** [1/10]", false);
        _formatter.FormatLogEntry("File1.cpp", false);
        
        // Act
        _formatter.Reset();
        var result = _formatter.FormatLogEntry("File2.cpp", false);

        // Assert
        // After reset, total is 0, so it should handle gracefully (likely [Compiled: 1/0] or similar based on logic)
        Assert.Equal("[Compiled: 1/0]", result.CompiledFilesText);
    }

    [Theory]
    [InlineData("Core.cpp")]
    [InlineData("Actor.cc")]
    [InlineData("Main.c")]
    [InlineData("Header.h")]
    [InlineData("Shader.ispc")]
    public void FormatLogEntry_DetectsVariousFileExtensions(string fileName)
    {
        // Arrange
        _formatter.FormatLogEntry("****** [1/1]", false);

        // Act
        var result = _formatter.FormatLogEntry(fileName, false);

        // Assert
        Assert.Equal("[Compiled: 1/1]", result.CompiledFilesText);
    }
}
