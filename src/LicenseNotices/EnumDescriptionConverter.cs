using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Avalonia.Data.Converters;

namespace MicMuter.LicenseNotices;

internal sealed class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Enum enumValue) return value?.ToString() ?? "";

        string enumStr = enumValue.ToString();
        FieldInfo field = enumValue.GetType().GetField(enumStr)!;
        var attributes = (DescriptionAttribute[])field.GetCustomAttributes<DescriptionAttribute>(false);
        return attributes.Length > 0 ? attributes[0].Description : enumStr;
    }
    
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType.IsEnum && value is string stringValue)
        {
            var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Static).AsSpan();
            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                if (attribute is not null && attribute.Description.Equals(stringValue, StringComparison.Ordinal))
                {
                    return Enum.Parse(targetType, field.Name);
                }
            }
        }
        
        return Enum.Parse(targetType, value!.ToString()!);
    }
}
