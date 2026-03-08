using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentAvalonia.UI.Controls;
using UnrealBinaryBuilder.Avalonia.Models;

namespace UnrealBinaryBuilder.Avalonia.Converters;

public class BuildStageToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is BuildStage stage && parameter is string targetStageStr && Enum.TryParse<BuildStage>(targetStageStr, out var targetStage))
        {
            if (stage == BuildStage.Failed) return Symbol.Dismiss;
            if (stage == targetStage) return Symbol.Play; // Or use a spinner
            if (stage > targetStage || stage == BuildStage.Finished) return Symbol.Checkmark;
        }
        return Symbol.Remove; // Pending
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
