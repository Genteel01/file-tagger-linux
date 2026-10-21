using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FileTagger.Statics;

namespace FileTagger.Converters;

public class ColourBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ColourScheme.Colours colour)
        {
            return new SolidColorBrush(ColourScheme.AccentColours[colour]);
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}