using System.Globalization;
using Avalonia.Styling;
using FileTagger.Converters;

namespace FileTaggerTests.Converters;

public class AreEqualConverterTests
{
    private AreEqualConverter GetConverter()
    {
        return new AreEqualConverter();
    }

    public static IList<object[]> SuccessTestData =>
        new List<object[]>
        {
            new object[] { "Same", new string(['S', 'a', 'm', 'e']) },
            new object[] { 1, 1 },
            new object[] { true, true },
            new object[] { ThemeVariant.Default, ThemeVariant.Default },
        };

    public static IList<object[]> FailureTestData =>
        new List<object[]>
        {
            new object[] { "Same", new string(['s', 'a', 'm', 'e']) },
            new object[] { 1, 2 },
            new object[] { true, false },
            new object[] { ThemeVariant.Default, ThemeVariant.Light },
            new object[] { 1, 1L },
        };

    [Theory]
    [MemberData(nameof(SuccessTestData))]
    public void Convert_EqualValues_ReturnsTrue(object? expectedValue, object? value)
    {
        object? result = GetConverter().Convert(
            [expectedValue, value],
            typeof(bool),
            null,
            CultureInfo.InvariantCulture);

        Assert.Equal(true, result);
    }

    [Theory]
    [MemberData(nameof(FailureTestData))]
    public void Convert_UnequalValues_ReturnsFalse(object? expectedValue, object? value)
    {
        object? result = GetConverter().Convert(
            [expectedValue, value],
            typeof(bool),
            null,
            CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }
}
