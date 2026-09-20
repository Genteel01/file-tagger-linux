using Avalonia.Styling;

namespace FileTagger.Assets.Statics;

public static class MyThemes
{
    public static readonly ThemeVariant LightGreen = new ThemeVariant(nameof(LightGreen), ThemeVariant.Light);
    public static readonly ThemeVariant DarkGreen = new ThemeVariant(nameof(DarkGreen), ThemeVariant.Dark);

    public static ThemeVariant StringToTheme(string themeName) => themeName switch
    {
        nameof(ThemeVariant.Light) => ThemeVariant.Light,
        nameof(ThemeVariant.Dark) => ThemeVariant.Dark,
        nameof(LightGreen) => LightGreen,
        nameof(DarkGreen) => DarkGreen,
        _ => ThemeVariant.Default
    };
}