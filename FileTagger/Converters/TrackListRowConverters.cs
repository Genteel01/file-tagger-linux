using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;

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
public class RowBorderBrushConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count > 0)
        {
            bool canUseValue = values.Count >= 2 && values[1] != null && values[1] is ThemeVariant;
            ThemeVariant themeVariant = canUseValue ? (ThemeVariant) values[1]! : ThemeVariant.Default;
            if (values[0] is true)
            {
                object? changedColour = TryGetResource("TrackChangedIndicationBrush", themeVariant);
                return changedColour ?? new SolidColorBrush(Color.Parse("#88FF0000"));
            }
            object? separatorColour = TryGetResource("ListSeparatorBrush", themeVariant);
            return separatorColour ?? new SolidColorBrush(Color.Parse("#88888888"));
        }
        return new SolidColorBrush(Color.Parse("#88888888"));
    }

    private object? TryGetResource(string key, ThemeVariant themeVariant)
    {
        if(Application.Current == null) return null;
        if(themeVariant == ThemeVariant.Default) themeVariant = Application.Current.ActualThemeVariant;
        Application.Current.Resources.TryGetResource(key, themeVariant, out object? separatorColour);
        return separatorColour;
    }
}