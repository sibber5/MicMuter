using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace MicMuter.LicenseNotices;

public sealed class LicenseTextMultiConverter : IMultiValueConverter
{
    // value[0]: license (enum)
    // value[1]: year
    // value[2]: copyright holders
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!targetType.IsAssignableFrom(typeof(string))) throw new ArgumentException("Invalid target type.", nameof(targetType));
        if (values.Count != 3) throw new ArgumentException("Invalid number of values.", nameof(values));
        if (values[0] is not (License or UnsetValueType)
            || values[1] is not (string or UnsetValueType or null)
            || values[2] is not (string or UnsetValueType or null))
            throw new ArgumentException("Invalid value types.", nameof(values));
        
        if (values[0] is UnsetValueType || values[1] is UnsetValueType || values[2] is UnsetValueType) return BindingOperations.DoNothing;

        License license = (License)values[0]!;
        
        if (license == License.None) return string.Empty;
        
        return string.Format(license switch
        {
            License.MIT => LicenseTemplates.MIT,
            _ => throw new NotImplementedException()
        }, (string?)values[1], (string?)values[2]);
    }
}
