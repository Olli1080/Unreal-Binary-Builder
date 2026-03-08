using CommunityToolkit.Mvvm.ComponentModel;

namespace UnrealBinaryBuilder.Avalonia.Models;

public class PluginPlatformWrapper : ObservableObject
{
    private bool _isChecked;
    public string Name { get; init; }
    public bool IsChecked { get => _isChecked; set => SetProperty(ref _isChecked, value); }
    public PluginPlatformWrapper(string name, bool isChecked = false) { Name = name; IsChecked = isChecked; }
}
