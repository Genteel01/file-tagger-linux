using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace FileTagger.Converters;

/// <summary>
/// Scales a given number value by a given scale factor. Can currently return a double or a Thickness.
/// </summary>
public class NumberPercentageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is null) return Activator.CreateInstance(targetType);

        double scaledValue;

        switch (value)
        {
            case double:
            case float:
            case sbyte:
            case byte:
            case short:
            case ushort:
            case int:
            case uint:
            case long:
            case ulong:
                scaledValue = System.Convert.ToDouble(value);
                break;
            default:
                return Activator.CreateInstance(targetType);
        }

        if (parameter is string scaleString)
        {
            if (double.TryParse(scaleString, out double scale))
            {
                scaledValue *= scale;
            }
        }

        if(targetType == typeof(double)) return scaledValue;
        if(targetType == typeof(Thickness)) return new Thickness(scaledValue);
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}