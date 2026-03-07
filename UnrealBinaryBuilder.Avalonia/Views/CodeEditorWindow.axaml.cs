using Avalonia.Controls;
using Avalonia.Interactivity;
using System.IO;
using System;

namespace UnrealBinaryBuilder.Avalonia.Views;

public partial class CodeEditorWindow : Window
{
    private string? _filePath;
    private bool _isDirty;

    public CodeEditorWindow()
    {
        InitializeComponent();
    }

    public bool LoadFile(string filePath)
    {
        if (!File.Exists(filePath)) return false;

        _filePath = filePath;
        TextEditor.Text = File.ReadAllText(filePath);
        
        var fileInfo = new FileInfo(filePath);
        if (fileInfo.IsReadOnly)
        {
            TextEditor.IsReadOnly = true;
            SaveBtn.IsEnabled = false;
        }

        _isDirty = false;
        UpdateTitle();
        return true;
    }

    private void UpdateTitle()
    {
        Title = _isDirty ? $"Code Editor - {Path.GetFileName(_filePath)} (Modified)" : $"Code Editor - {Path.GetFileName(_filePath)}";
    }

    private void SaveBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (_filePath == null) return;
        File.WriteAllText(_filePath, TextEditor.Text);
        _isDirty = false;
        UpdateTitle();
    }

    private void CloseBtn_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TextEditor_TextChanged(object? sender, EventArgs e)
    {
        if (!_isDirty)
        {
            _isDirty = true;
            UpdateTitle();
        }
    }
}
