using Avalonia.Styling;

namespace FileTagger.Statics;

public static class Themes
{
    public enum LayoutSize
    {
        Automatic,
        Standard,
        Compact,
    }

    public static ThemeVariant StringToTheme(string themeName) => themeName switch
    {
        nameof(ThemeVariant.Light) => ThemeVariant.Light,
        nameof(ThemeVariant.Dark) => ThemeVariant.Dark,
        _ => ThemeVariant.Default
    };

    #if DEBUG
    public static ThemeVariant GetOppositeTheme(ThemeVariant theme) => GetOppositeTheme(theme.ToString());

    private static ThemeVariant GetOppositeTheme(string themeName) => themeName switch
    {
        nameof(ThemeVariant.Light) => ThemeVariant.Dark,
        nameof(ThemeVariant.Dark) => ThemeVariant.Light,
        _ => ThemeVariant.Dark
    };
    #endif
}