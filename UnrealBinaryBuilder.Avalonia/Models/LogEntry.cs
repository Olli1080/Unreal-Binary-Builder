using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UnrealBinaryBuilder.Avalonia.Models;

public enum LogMessageType
{
    Info,
    Debug,
    Warning,
    Error
}

public partial class LogEntry : ObservableObject
{
    [ObservableProperty]
    private DateTime _dateTime;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private LogMessageType _messageType = LogMessageType.Info;
}
