using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace FileTagger.Converters;

/// <summary>
/// Returns True if all values are equal, returns false otherwise. Returns True if there are 0 or 1 values.
/// </summary>
public class AreEqualConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2)
        {
            object? firstValue = values[0];
            for (int i = 1; i < values.Count; i++)
            {
                if (values[i] != firstValue) return false;
            }
        }
        return true;
    }
}