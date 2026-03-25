namespace UnrealBinaryBuilder.Avalonia.Classes;

public static class TelemetryConstants
{
    public const string EVENT_BUILD_STARTED = "Build:Started";
    public const string EVENT_BUILD_FINISHED = "Build:Finished";
    public const string EVENT_ZIP_STARTED = "Zip:Started";
    public const string EVENT_ZIP_FINISHED = "Zip:Finished";
    public const string EVENT_SHUTDOWN_STARTED = "Shutdown:Started";
    public const string EVENT_PROGRAM_START_DEBUG = "Program:Start:Debug";
    public const string EVENT_PROGRAM_START_RELEASE = "Program:Start:Release";

    public const string CAT_BUILD = "Build";
    public const string STEP_ENGINE = "Engine";
    public const string STEP_PLUGIN = "Plugin";
}
