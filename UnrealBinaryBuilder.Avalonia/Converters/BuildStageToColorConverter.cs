using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Converters;

public class BuildStageToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is BuildStage stage && parameter is string targetStageStr && Enum.TryParse<BuildStage>(targetStageStr, out var targetStage))
        {
            if (stage == BuildStage.Failed) return Brushes.Red;
            if (stage == targetStage) return Brushes.DodgerBlue; // Running
            if (stage > targetStage || stage == BuildStage.Finished) return Brushes.Green; // Completed
        }
        return Brushes.Gray; // Idle/Pending
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
