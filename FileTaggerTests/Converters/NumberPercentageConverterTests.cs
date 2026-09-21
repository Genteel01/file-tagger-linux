using System.Globalization;
using Avalonia;
using FileTagger.Converters;

namespace FileTaggerTests.Converters;

public class NumberPercentageConverterTests
{
    private NumberPercentageConverter GetConverter()
    {
        return new NumberPercentageConverter();
    }

    const string TestScale = "0.5";

    public static IList<object> DoubleTestData => new List<object>
    {
        new TheoryDataRow<double, double>(25, 50),
        new TheoryDataRow<double, float>(25, 50),
        new TheoryDataRow<double, sbyte>(25, 50),
        new TheoryDataRow<double, byte>(25, 50),
        new TheoryDataRow<double, short>(25, 50),
        new TheoryDataRow<double, ushort>(25, 50),
        new TheoryDataRow<double, int>(25, 50),
        new TheoryDataRow<double, uint>(25, 50),
        new TheoryDataRow<double, long>(25, 50),
        new TheoryDataRow<double, ulong>(25, 50),
    };

    public static IList<object> ThicknessTestData => new List<object>
    {
        new TheoryDataRow<Thickness, double>(new Thickness(25), 50),
        new TheoryDataRow<Thickness, float>(new Thickness(25), 50),
        new TheoryDataRow<Thickness, sbyte>(new Thickness(25), 50),
        new TheoryDataRow<Thickness, byte>(new Thickness(25), 50),
        new TheoryDataRow<Thickness, short>(new Thickness(25), 50),
        new TheoryDataRow<Thickness, ushort>(new Thickness(25), 50),
        new TheoryDataRow<Thickness, int>(new Thickness(25), 50),
        new TheoryDataRow<Thickness, uint>(new Thickness(25), 50),
        new TheoryDataRow<Thickness, long>(new Thickness(25), 50),
        new TheoryDataRow<Thickness, ulong>(new Thickness(25), 50),
    };

    [Theory]
    [MemberData(nameof(DoubleTestData))]
    public void Convert_NumberWithScale_ReturnsScaledDouble(double expectedValue, object? value)
    {
        object? result = GetConverter().Convert(value, typeof(double), TestScale, CultureInfo.InvariantCulture);

        Assert.Equal(expectedValue, result);
    }

    [Theory]
    [MemberData(nameof(ThicknessTestData))]
    public void Convert_NumberToThickness_ReturnsUniformThickness(Thickness expectedValue, object? value)
    {
        object? result = GetConverter().Convert(value, typeof(Thickness), TestScale, CultureInfo.InvariantCulture);

        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void Convert_NumberWithoutScale_ReturnsUnscaledDouble()
    {
        object? result = GetConverter().Convert(12.5f, typeof(double), null, CultureInfo.InvariantCulture);

        Assert.Equal(12.5d, result);
    }

    [Fact]
    public void Convert_InvalidScale_ReturnsUnscaledDouble()
    {
        object? result = GetConverter().Convert(12, typeof(double), "invalid", CultureInfo.InvariantCulture);

        Assert.Equal(12d, result);
    }

    [Fact]
    public void Convert_UnsupportedValue_ReturnsDefaultTargetValue()
    {
        object? result = GetConverter().Convert("12", typeof(double), null, CultureInfo.InvariantCulture);

        Assert.Equal(0d, result);
    }

    [Fact]
    public void Convert_NullValue_ReturnsDefaultTargetValue()
    {
        object? result = GetConverter().Convert(null, typeof(double), null, CultureInfo.InvariantCulture);

        Assert.Equal(0d, result);
    }

    [Fact]
    public void Convert_UnsupportedTargetType_ReturnsOriginalValue()
    {
        const int value = 12;

        object? result = GetConverter().Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal(value, result);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        Assert.Throws<NotImplementedException>(() =>
            GetConverter().ConvertBack(12d, typeof(double), null, CultureInfo.InvariantCulture));
    }
}