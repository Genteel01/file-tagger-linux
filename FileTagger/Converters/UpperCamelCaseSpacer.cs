using System;
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
/// Converts a given <see cref="PictureInfo.PIC_TYPE"/> or <see cref="ThemeVariant"/> into a string with spaces
/// where the capital letters are, e.g. "LightGreen" to "Light Green". Ignores fully capital words. Also converts back.
/// </summary>
public class UpperCamelCaseSpacer : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is PictureInfo.PIC_TYPE picType)
        {
            return SpaceUpperCamelCaseString(picType.ToString());
        }

        if (value is IEnumerable<PictureInfo.PIC_TYPE> picTypes)
        {
            List<string> stringList = [];
            picTypes.ToList().ForEach(pt => stringList.Add(SpaceUpperCamelCaseString(pt.ToString())));
            return stringList;
        }

        if (value is ThemeVariant variant)
        {
            return SpaceUpperCamelCaseString(variant.ToString());
        }

        if (value is IEnumerable<ThemeVariant> variants)
        {
            List<string> stringList = [];
            variants.ToList().ForEach(v => stringList.Add(SpaceUpperCamelCaseString(v.ToString())));
            return stringList;
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
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