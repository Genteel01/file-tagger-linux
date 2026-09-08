using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace FileTagger.Converters;

/// <summary>
/// Converts between <see cref="double"/> and <see cref="GridLength"/>, so doubles can be used to bind grid row/column sizes
/// </summary>
public class GridSizeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return new GridLength((double) value!, GridUnitType.Pixel);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ((GridLength) value!).Value;
    }
}