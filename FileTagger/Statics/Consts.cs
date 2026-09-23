using System;
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

    public static ThemeVariant GetOppositeTheme(string themeName) => themeName switch
    {
        nameof(ThemeVariant.Light) => ThemeVariant.Dark,
        nameof(ThemeVariant.Dark) => ThemeVariant.Light,
        nameof(LightGreen) => DarkGreen,
        nameof(DarkGreen) => LightGreen,
        _ => ThemeVariant.Dark
    };
}

public static class Consts
{
    /// <summary>
    /// Value for edit fields that we don't want to change
    /// </summary>
    public const string UnchangedField = "< keep >";
}

public static class Maths
{
    public static int ChangeCollectionIndex(int currentIndex, int collectionCount, int change)
    {
        int newIndex = currentIndex + change;
        if (newIndex >= collectionCount) return newIndex % collectionCount;
        if (newIndex < 0) return newIndex + collectionCount;
        return newIndex;
    }
}