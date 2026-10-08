using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace FileTagger.Statics;

public static class ColourScheme
{
    public enum Colours
    {
        Blue,
        Green
    }

    private static readonly Dictionary<Colours, Color> ThemeAccentColor0 = new Dictionary<Colours, Color>
    {
        { Colours.Blue, Color.FromArgb(0xFF, 0x00, 0xBF, 0xFF) },
        { Colours.Green, Color.FromArgb(0xFF, 0x00, 0xE6, 0x45) }
    };

    private static readonly Dictionary<Colours, Color> ThemeAccentColor1 = new Dictionary<Colours, Color>
    {
        { Colours.Blue, Color.FromArgb(0xCC, 0x00, 0xBF, 0xFF) },
        { Colours.Green, Color.FromArgb(0xCC, 0x00, 0xE6, 0x45) }
    };

    private static readonly Dictionary<Colours, Color> ThemeAccentColor2 = new Dictionary<Colours, Color>
    {
        { Colours.Blue, Color.FromArgb(0x99, 0x00, 0xBF, 0xFF) },
        { Colours.Green, Color.FromArgb(0x99, 0x00, 0xE6, 0x45) }
    };

    private static readonly Dictionary<Colours, Color> ThemeAccentColor3 = new Dictionary<Colours, Color>
    {
        { Colours.Blue, Color.FromArgb(0x66, 0x00, 0xBF, 0xFF) },
        { Colours.Green, Color.FromArgb(0x66, 0x00, 0xE6, 0x45) }
    };

    private static readonly Dictionary<Colours, Color> ThemeAccentColor4 = new Dictionary<Colours, Color>
    {
        { Colours.Blue, Color.FromArgb(0x33, 0x00, 0xBF, 0xFF) },
        { Colours.Green, Color.FromArgb(0x33, 0x00, 0xE6, 0x45) }
    };

    private static readonly Dictionary<Colours, Color> HighlightColor = new Dictionary<Colours, Color>
    {
        { Colours.Blue, Color.FromArgb(0xFF, 0x00, 0x5F, 0x7F) },
        { Colours.Green, Color.FromArgb(0xFF, 0x00, 0x7F, 0x2F) }
    };

    private static readonly Dictionary<Colours, Color> HighlightColorDark = new Dictionary<Colours, Color>
    {
        { Colours.Blue, ThemeAccentColor0[Colours.Blue] },
        { Colours.Green, ThemeAccentColor0[Colours.Green] }
    };

    private static readonly Dictionary<Colours, Color> FocusedTextBoxTrackColor = new Dictionary<Colours, Color>
    {
        { Colours.Blue, Color.FromArgb(0xFF, 0xCC, 0xF2, 0xFF) },
        { Colours.Green, Color.FromArgb(0xFF, 0xCC, 0xFA, 0xDA) }
    };

    private static readonly Dictionary<Colours, Color> FocusedTextBoxTrackColorDark = new Dictionary<Colours, Color>
    {
        { Colours.Blue, Color.FromArgb(0xFF, 0x0C, 0x32, 0x3F) },
        { Colours.Green, Color.FromArgb(0xFF, 0x0C, 0x3A, 0x1A) }
    };

    /// <summary>
    /// Sets all accent colour resources to the value for the given colour
    /// </summary>
    public static void ApplyColoursToApp(Application app, Colours colour)
    {
        app.Resources["ThemeAccentColor0"] = ThemeAccentColor0[colour];
        app.Resources["ThemeAccentColor"] = ThemeAccentColor1[colour];
        app.Resources["ThemeAccentColor2"] = ThemeAccentColor2[colour];
        app.Resources["ThemeAccentColor3"] = ThemeAccentColor3[colour];
        app.Resources["ThemeAccentColor4"] = ThemeAccentColor4[colour];

        if (app.ActualThemeVariant == ThemeVariant.Dark)
        {
            app.Resources["HighlightColor"] = HighlightColorDark[colour];
            app.Resources["FocusedTextBoxTrackColor"] = FocusedTextBoxTrackColorDark[colour];
        }
        else
        {
            app.Resources["HighlightColor"] = HighlightColor[colour];
            app.Resources["FocusedTextBoxTrackColor"] = FocusedTextBoxTrackColor[colour];
        }
    }
}