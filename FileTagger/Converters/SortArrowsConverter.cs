using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace FileTagger.Converters;

/// <summary>
/// Converts a string to a bool, if the parameter is also the same string
/// </summary>
public class SortArrowsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string sort && parameter is string name)
        {
            return name == sort;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}