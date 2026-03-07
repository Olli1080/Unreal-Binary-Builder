using Avalonia.Controls;
using UnrealBinaryBuilder.Avalonia.ViewModels;
using System.ComponentModel;
using FluentAvalonia.UI.Controls;
using System;
using global::Avalonia.Controls.Notifications;
using UnrealBinaryBuilder.Avalonia.Classes;
using UnrealBinaryBuilder.Avalonia.Models;

// Resolve namespace collision between Project and Framework
using AvaloniaNotificationType = global::Avalonia.Controls.Notifications.NotificationType;

namespace UnrealBinaryBuilder.Avalonia.Views;

public partial class MainWindow : Window
{
    private WindowNotificationManager? _notificationManager;

    public MainWindow()
    {
        InitializeComponent();
        _notificationManager = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.BottomRight,
            MaxItems = 3
        };

        DataContextChanged += OnDataContextChanged;
        Closing += MainWindow_Closing;
        
        // Restore window state
        var settings = BuilderSettings.GetSettingsFile();
        if (settings != null)
        {
            if (settings.WindowWidth > 0) Width = settings.WindowWidth;
            if (settings.WindowHeight > 0) Height = settings.WindowHeight;
            if (settings.WindowLeft.HasValue && settings.WindowTop.HasValue)
            {
                Position = new global::Avalonia.PixelPoint((int)settings.WindowLeft.Value, (int)settings.WindowTop.Value);
            }
            if (settings.bWindowMaximized) WindowState = WindowState.Maximized;
        }
    }

    private async void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            if (vm.IsBuilding)
            {
                e.Cancel = true;

                var dialog = new ContentDialog
                {
                    Title = "Build in progress",
                    Content = "A build is still running. Would you like to stop it and exit?",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "No",
                    DefaultButton = ContentDialogButton.Close
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    vm.IsBuilding = false;
                    FinalizeExit(vm);
                }
                return;
            }
            
            FinalizeExit(vm);
        }
    }

    private void FinalizeExit(MainWindowViewModel vm)
    {
        // Save window state
        vm.Settings.WindowWidth = Width;
        vm.Settings.WindowHeight = Height;
        vm.Settings.WindowLeft = Position.X;
        vm.Settings.WindowTop = Position.Y;
        vm.Settings.bWindowMaximized = WindowState == WindowState.Maximized;
        
        BuilderSettings.SaveSettings(vm.Settings);
        GameAnalyticsCSharp.EndSession();
        
        Closing -= MainWindow_Closing;
        Close();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;
            vm.ShowNotification += OnShowNotification;
        }
    }

    private void OnShowNotification(object? sender, NotificationEventArgs e)
    {
        AvaloniaNotificationType targetType = e.Type switch
        {
            UBBNotificationType.Success => AvaloniaNotificationType.Success,
            UBBNotificationType.Warning => AvaloniaNotificationType.Warning,
            UBBNotificationType.Error => AvaloniaNotificationType.Error,
            _ => AvaloniaNotificationType.Information
        };

        _notificationManager?.Show(new Notification(e.Title, e.Message, targetType));
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.LogText))
        {
            if (DataContext is MainWindowViewModel vm)
            {
                if (Editor != null)
                {
                    Editor.Text = vm.LogText;
                    Editor.CaretOffset = Editor.Text.Length;
                    Editor.ScrollToLine(Editor.LineCount);
                }
                
                if (LogViewEditor != null)
                {
                    LogViewEditor.Text = vm.LogText;
                }
            }
        }
    }
}
