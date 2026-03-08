using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UnrealBinaryBuilder.Avalonia.Models;

public class GameConfigWrapper : ObservableObject
{
    private readonly HashSet<BuildConfiguration> _configurations;
    private readonly BuildConfiguration _config;
    private readonly Action _onChanged;

    public GameConfigWrapper(HashSet<BuildConfiguration> configurations, BuildConfiguration config, Action onChanged)
    {
        _configurations = configurations;
        _config = config;
        _onChanged = onChanged;
    }

    public bool IsChecked
    {
        get => _configurations.Contains(_config);
        set
        {
            if (value) _configurations.Add(_config);
            else _configurations.Remove(_config);
            OnPropertyChanged();
            _onChanged();
        }
    }

    public string Name => _config.ToString();
}
