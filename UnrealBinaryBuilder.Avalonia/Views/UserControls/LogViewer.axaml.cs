using System;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace UnrealBinaryBuilder.Avalonia.Views.UserControls;

public partial class LogViewer : UserControl
{
    private int _logCount = 0;
    private bool _autoScroll = true;

    public enum EMessageType
    {
        Info,
        Debug,
        Warning,
        Error
    }

    public LogViewer()
    {
        InitializeComponent();
        LogTextEditor.Options.EnableVirtualSpace = false;
        LogTextEditor.Options.AllowScrollBelowDocument = false;
        LogTextEditor.TextArea.Caret.CaretBrush = Brushes.Transparent;
    }

    public void AddLogEntry(string message, EMessageType messageType = EMessageType.Info)
    {
        Dispatcher.UIThread.Post(() =>
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string prefix = messageType switch
            {
                EMessageType.Debug => "[DEBUG] ",
                EMessageType.Warning => "[WARNING] ",
                EMessageType.Error => "[ERROR] ",
                _ => ""
            };

            string fullMessage = $"[{timestamp}] {prefix}{message}{Environment.NewLine}";
            LogTextEditor.Document.Insert(LogTextEditor.Document.TextLength, fullMessage);
            
            _logCount++;
            LogCountTextBlock.Text = $"Total log entries: {_logCount}";

            if (_autoScroll)
            {
                LogTextEditor.ScrollToEnd();
            }
        });
    }

    public void ClearAllLogs()
    {
        Dispatcher.UIThread.Post(() =>
        {
            LogTextEditor.Document.Text = string.Empty;
            _logCount = 0;
            LogCountTextBlock.Text = $"Total log entries: 0";
        });
    }

    public void SetAutoScroll(bool enabled)
    {
        _autoScroll = enabled;
    }
}
