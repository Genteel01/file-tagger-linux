using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace FileTagger.Converters;

/// <summary>
/// Converts a bool binding to border thickness. Used to visualise modified rows
/// </summary>
public class RowBorderThicknessConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true)
        {
            return new Thickness(0, 1);
        }

        return new Thickness(0, 0, 0, 1);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts a bool binding to border brush. Used to visualise modified rows
/// </summary>
public class RowBorderBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true)
        {
            return new SolidColorBrush(Colors.Red);
        }

        if (parameter is ImmutableSolidColorBrush brush)
        {
            return brush;
        }
        return new SolidColorBrush(Colors.LightGray);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}