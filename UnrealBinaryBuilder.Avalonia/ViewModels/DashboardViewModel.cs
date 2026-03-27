using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using UnrealBinaryBuilder.Avalonia.Classes.Interfaces;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IBuildHistoryService _historyService;

    [ObservableProperty] private int _totalBuilds;
    [ObservableProperty] private int _successfulBuilds;
    [ObservableProperty] private int _failedBuilds;
    [ObservableProperty] private string _successRate = "0%";
    [ObservableProperty] private string _averageDuration = "00:00:00";
    [ObservableProperty] private string _totalTimeSaved = "0h"; // Assuming UBB saves time compared to manual

    public DashboardViewModel(IBuildHistoryService historyService)
    {
        _historyService = historyService;
        _ = RefreshStatsAsync();
    }

    public async Task RefreshStatsAsync()
    {
        var history = await _historyService.GetHistoryAsync();
        var entries = history.ToList();

        TotalBuilds = entries.Count;
        SuccessfulBuilds = entries.Count(e => e.IsSuccess);
        FailedBuilds = TotalBuilds - SuccessfulBuilds;

        if (TotalBuilds > 0)
        {
            SuccessRate = $"{(double)SuccessfulBuilds / TotalBuilds:P0}";
            
            var avgTicks = entries.Where(e => e.IsSuccess).Average(e => e.Duration.Ticks);
            AverageDuration = TimeSpan.FromTicks((long)avgTicks).ToString(@"hh\:mm\:ss");

            // Hypothetical: Each successful UBB build saves ~30 mins of manual effort
            double hoursSaved = SuccessfulBuilds * 0.5;
            TotalTimeSaved = $"{hoursSaved:F1}h";
        }
    }
}
