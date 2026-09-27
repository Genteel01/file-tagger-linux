using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ATL;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using FileTagger.Statics;

namespace FileTagger.Converters;

/// <summary>
/// Converts a given object into a string with spaces where the capital letters are, e.g. "LightGreen" to "Light Green".
/// Ignores fully capital words. Handles lists by returning a list of strings.
/// Also converts back for String, PictureInfo.PIC_TYPE, and ThemeVariant
/// </summary>
public class UpperCamelCaseSpacer : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        //String is an IEnumerable, so we need to check for that separately
        if (value is string stringValue) return SpaceUpperCamelCaseString(stringValue);
        if (value is IEnumerable values)
        {
            List<string> stringList = [];
            foreach (object o in values)
            {
                stringList.Add(SpaceUpperCamelCaseString(o.ToString() ?? ""));
            }
            return stringList;
        }
        if (value?.ToString() is null) return null;
        return SpaceUpperCamelCaseString(value.ToString()!);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType == typeof(string))
        {
            if (value is string stringValue)
            {
                return stringValue.Replace(" ", "");
            }
        }

        if (targetType == typeof(PictureInfo.PIC_TYPE))
        {
            if (value is string stringValue)
            {
                return StringToPicType(stringValue);
            }
        }

        if (targetType == typeof(ThemeVariant))
        {
            if (value is string stringValue)
            {
                return StringToThemeVariant(stringValue);
            }
        }

        return value;
    }

    private string SpaceUpperCamelCaseString(string stringValue)
    {
        if (string.IsNullOrEmpty(stringValue) || stringValue.All(char.IsUpper)) return stringValue;

        StringBuilder result = new StringBuilder();
        result.Append(stringValue[0]);

        for (int i = 1; i < stringValue.Length; i++)
        {
            if (char.IsUpper(stringValue[i]))
            {
                result.Append(' ');
            }
            result.Append(stringValue[i]);
        }

        return result.ToString();
    }

    private PictureInfo.PIC_TYPE StringToPicType(string stringValue)
    {
        string gaplessString = stringValue.Replace(" ", "");
        return Enum.Parse<PictureInfo.PIC_TYPE>(gaplessString);
    }

    private ThemeVariant StringToThemeVariant(string stringValue)
    {
        string gaplessString = stringValue.Replace(" ", "");
        return MyThemes.StringToTheme(gaplessString);
    }
}