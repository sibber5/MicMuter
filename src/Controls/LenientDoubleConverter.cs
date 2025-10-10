using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace MicMuter.Controls;

internal sealed class LenientDoubleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double val) return AvaloniaProperty.UnsetValue;
        return val.ToString("F0", CultureInfo.CurrentCulture);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str) return 0.0;

        if (string.IsNullOrEmpty(str) || str == "-") return 0.0;

        if (!double.TryParse(str, NumberStyles.Float, CultureInfo.CurrentCulture, out var val)) return 0.0;

        return val;
    }
}
