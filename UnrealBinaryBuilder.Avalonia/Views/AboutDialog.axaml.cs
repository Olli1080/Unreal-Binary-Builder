using Avalonia.Controls;
using Avalonia.Interactivity;

namespace UnrealBinaryBuilder.Avalonia.Views;

public partial class AboutDialog : Window
{
    public AboutDialog()
    {
        InitializeComponent();
    }

    private void CloseBtn_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
