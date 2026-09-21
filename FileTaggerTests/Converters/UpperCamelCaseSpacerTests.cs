using System.Globalization;
using ATL;
using Avalonia.Styling;
using FileTagger.Assets.Statics;
using FileTagger.Converters;

namespace FileTaggerTests.Converters;

public class UpperCamelCaseSpacerTests
{
    private readonly UpperCamelCaseSpacer _converter = new();

    [Fact]
    public void Convert_PictureType_SpacesCamelCase()
    {
        object? result = _converter.Convert(PictureInfo.PIC_TYPE.RecordingLocation, typeof(string), null,
            CultureInfo.InvariantCulture);

        Assert.Equal("Recording Location", result);
    }

    [Fact]
    public void Convert_ThemeVariant_SpacesCamelCase()
    {
        object? result = _converter.Convert(MyThemes.LightGreen, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("Light Green", result);
    }

    [Fact]
    public void Convert_RawUppercaseString_ReturnsOriginalValue()
    {
        object? result = _converter.Convert("URL", typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("URL", result);
    }

    [Fact]
    public void ConvertBack_PictureType_StripsSpaces()
    {
        object? result = _converter.ConvertBack("Recording Location", typeof(PictureInfo.PIC_TYPE), null,
            CultureInfo.InvariantCulture);

        Assert.Equal(PictureInfo.PIC_TYPE.RecordingLocation, result);
    }

    [Fact]
    public void ConvertBack_ThemeVariant_MapsKnownStrings()
    {
        object? result = _converter.ConvertBack("Light Green", typeof(ThemeVariant), null, CultureInfo.InvariantCulture);

        Assert.NotNull(result);
        Assert.Equal(nameof(MyThemes.LightGreen), result!.ToString());
    }

    [Fact]
    public void ConvertBack_UnknownThemeVariant_DefaultsToThemeVariantDefault()
    {
        object? result = _converter.ConvertBack("Custom Theme", typeof(ThemeVariant), null, CultureInfo.InvariantCulture);

        Assert.Equal(ThemeVariant.Default, result);
    }
}