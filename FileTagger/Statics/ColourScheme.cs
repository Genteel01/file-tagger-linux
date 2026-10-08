using System;
using System.Collections.Generic;
using System.Numerics;
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
        { Colours.Blue, Color.FromArgb(0xFF, 0x3D, 0xAE, 0xE9) },
        { Colours.Green, Color.FromArgb(0xFF, 0x3D, 0xD4, 0x25) }
    };

    private static readonly Dictionary<Colours, Color> ThemeAccentColor1 = new Dictionary<Colours, Color>
    {
        { Colours.Blue, Color.FromArgb(0xCC, 0x3D, 0xAE, 0xE9) },
        { Colours.Green, Color.FromArgb(0xCC, 0x3D, 0xD4, 0x25) }
    };

    private static readonly Dictionary<Colours, Color> ThemeAccentColor2 = new Dictionary<Colours, Color>
    {
        { Colours.Blue, Color.FromArgb(0x99, 0x3D, 0xAE, 0xE9) },
        { Colours.Green, Color.FromArgb(0x99, 0x3D, 0xD4, 0x25) }
    };

    private static readonly Dictionary<Colours, Color> ThemeAccentColor3 = new Dictionary<Colours, Color>
    {
        { Colours.Blue, Color.FromArgb(0x66, 0x3D, 0xAE, 0xE9) },
        { Colours.Green, Color.FromArgb(0x66, 0x3D, 0xD4, 0x25) }
    };

    private static readonly Dictionary<Colours, Color> ThemeAccentColor4 = new Dictionary<Colours, Color>
    {
        { Colours.Blue, Color.FromArgb(0x33, 0x3D, 0xAE, 0xE9) },
        { Colours.Green, Color.FromArgb(0x33, 0x3D, 0xD4, 0x25) }
    };

    private static readonly Dictionary<Colours, Color> HighlightColor = new Dictionary<Colours, Color>
    {
        { Colours.Blue, AdjustValue(ThemeAccentColor0[Colours.Blue], 0.5) },
        { Colours.Green, AdjustValue(ThemeAccentColor0[Colours.Green], 0.5) },
    };

    private static readonly Dictionary<Colours, Color> HighlightColorDark = new Dictionary<Colours, Color>
    {
        { Colours.Blue, ThemeAccentColor0[Colours.Blue] },
        { Colours.Green, ThemeAccentColor0[Colours.Green] }
    };

    //The background colour here is the ThemeBackgroundColour for light theme
    private static readonly Dictionary<Colours, Color> FocusedTextBoxTrackColor = new Dictionary<Colours, Color>
    {
        { Colours.Blue, CombineOverlayAndBase(ThemeAccentColor4[Colours.Blue], Colors.White) },
        { Colours.Green, CombineOverlayAndBase(ThemeAccentColor4[Colours.Green], Colors.White) },
    };
    //The background colour here is the ThemeBackgroundColour for dark theme
    private static readonly Dictionary<Colours, Color> FocusedTextBoxTrackColorDark = new Dictionary<Colours, Color>
    {
        { Colours.Blue, CombineOverlayAndBase(ThemeAccentColor4[Colours.Blue], Color.FromRgb(0x0F, 0x0F, 0x0F)) },
        { Colours.Green, CombineOverlayAndBase(ThemeAccentColor4[Colours.Green], Color.FromRgb(0x0F, 0x0F, 0x0F)) }
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

    /// <summary>
    /// Adjusts the RGB values of a colour based on a multiplier.
    /// Note if the new value is greater than 255, it will be set to 255.
    /// </summary>
    private static Color AdjustValue(Color color, double multiplier)
    {
        byte newR = (byte)Math.Min(color.R * multiplier, 255);
        byte newG = (byte)Math.Min(color.G * multiplier, 255);
        byte newB = (byte)Math.Min(color.B * multiplier, 255);
        return new Color(color.A, newR, newG, newB);
    }

    /// <summary>
    /// Takes a partially transparent overlay colour and a background colour and combines them into an opaque colour.
    /// Note currently it ignores the alpha channel of the background colour.
    /// </summary>
    private static Color CombineOverlayAndBase(Color overlay, Color background)
    {
        float opacity = (float)(overlay.A / 255.0);

        Vector3 baseColours = new Vector3(background.R, background.G, background.B);
        Vector3 overlayColours = new Vector3(overlay.R, overlay.G, overlay.B);

        Vector3 differences = baseColours - overlayColours;
        Vector3 differencesWithOpacity = differences * opacity;
        Vector3 result = baseColours - differencesWithOpacity;

        Color newColour = new Color(255, (byte)result.X, (byte)result.Y, (byte)result.Z);

        return newColour;

    }
}