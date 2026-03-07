using Avalonia.Controls;
using Avalonia.Interactivity;
using Sentry;
using System;
using FluentAvalonia.UI.Controls;

namespace UnrealBinaryBuilder.Avalonia.Views.UserControls;

public partial class CrashReporter : Window
{
    public SentryId CurrentSentryId;

    public CrashReporter(Exception InException)
    {
        InitializeComponent();
        Username.Text = Environment.UserName;
        var StackTraceMessage = $"Source ->\t{InException.Source}\nMessage ->\t{InException.Message}\nTarget ->\t{InException.TargetSite}\nStackTrace ->\n{InException.StackTrace}";
        StackTraceText.Text = StackTraceMessage;
    }

    public CrashReporter()
    {
        InitializeComponent();
    }

    private void SubmitBtn_Click(object? sender, RoutedEventArgs e)
    {
        var CommentText = $"{Comment.Text}\n\nExceptionDetails ->\n{StackTraceText.Text}";
        var userFeedback = new UserFeedback(CurrentSentryId, Username.Text, Email.Text, CommentText);
        SentrySdk.CaptureUserFeedback(userFeedback);
        
        ShowSuccessDialog();
    }

    private async void ShowSuccessDialog()
    {
        var dialog = new ContentDialog
        {
            Title = "Success",
            Content = "Thank you for submitting the crash report!",
            CloseButtonText = "OK"
        };
        await dialog.ShowAsync();
        Close();
    }

    private void CancelBtn_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
