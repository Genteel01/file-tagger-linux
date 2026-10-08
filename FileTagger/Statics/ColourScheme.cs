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
        Pink,
        Red,
        Orange,
        Yellow,
        Green,
        Teal,
        Blue,
        Indigo,
        Violet,
    }

    private static readonly Dictionary<Colours, Color> ThemeAccentColor0 = new Dictionary<Colours, Color>
    {
        { Colours.Pink, Color.FromArgb(0xFF, 0xE9, 0x3A, 0x9A) },
        { Colours.Red, Color.FromArgb(0xFF, 0xE9, 0x3D, 0x58) },
        { Colours.Orange, Color.FromArgb(0xFF, 0xE9, 0x64, 0x3A) },
        { Colours.Yellow, Color.FromArgb(0xFF, 0xE8, 0xCB, 0x2D) },
        { Colours.Green, Color.FromArgb(0xFF, 0x3D, 0xD4, 0x25) },
        { Colours.Teal, Color.FromArgb(0xFF, 0x00, 0xD3, 0xB8) },
        { Colours.Blue, Color.FromArgb(0xFF, 0x3D, 0xAE, 0xE9) },
        { Colours.Indigo, Color.FromArgb(0xFF, 0x92, 0x6E, 0xE4) },
        { Colours.Violet, Color.FromArgb(0xFF, 0xB8, 0x75, 0xDC) }
    };

    private static readonly Dictionary<Colours, Color> ThemeAccentColor1 = new Dictionary<Colours, Color>
    {
        { Colours.Pink, Color.FromArgb(0xCC, 0xE9, 0x3A, 0x9A) },
        { Colours.Red, Color.FromArgb(0xCC, 0xE9, 0x3D, 0x58) },
        { Colours.Orange, Color.FromArgb(0xCC, 0xE9, 0x64, 0x3A) },
        { Colours.Yellow, Color.FromArgb(0xCC, 0xE8, 0xCB, 0x2D) },
        { Colours.Green, Color.FromArgb(0xCC, 0x3D, 0xD4, 0x25) },
        { Colours.Teal, Color.FromArgb(0xCC, 0x00, 0xD3, 0xB8) },
        { Colours.Blue, Color.FromArgb(0xCC, 0x3D, 0xAE, 0xE9) },
        { Colours.Indigo, Color.FromArgb(0xCC, 0x92, 0x6E, 0xE4) },
        { Colours.Violet, Color.FromArgb(0xCC, 0xB8, 0x75, 0xDC) }
    };

    private static readonly Dictionary<Colours, Color> ThemeAccentColor2 = new Dictionary<Colours, Color>
    {
        { Colours.Pink, Color.FromArgb(0x99, 0xE9, 0x3A, 0x9A) },
        { Colours.Red, Color.FromArgb(0x99, 0xE9, 0x3D, 0x58) },
        { Colours.Orange, Color.FromArgb(0x99, 0xE9, 0x64, 0x3A) },
        { Colours.Yellow, Color.FromArgb(0x99, 0xE8, 0xCB, 0x2D) },
        { Colours.Green, Color.FromArgb(0x99, 0x3D, 0xD4, 0x25) },
        { Colours.Teal, Color.FromArgb(0x99, 0x00, 0xD3, 0xB8) },
        { Colours.Blue, Color.FromArgb(0x99, 0x3D, 0xAE, 0xE9) },
        { Colours.Indigo, Color.FromArgb(0x99, 0x92, 0x6E, 0xE4) },
        { Colours.Violet, Color.FromArgb(0x99, 0xB8, 0x75, 0xDC) }
    };

    private static readonly Dictionary<Colours, Color> ThemeAccentColor3 = new Dictionary<Colours, Color>
    {
        { Colours.Pink, Color.FromArgb(0x66, 0xE9, 0x3A, 0x9A) },
        { Colours.Red, Color.FromArgb(0x66, 0xE9, 0x3D, 0x58) },
        { Colours.Orange, Color.FromArgb(0x66, 0xE9, 0x64, 0x3A) },
        { Colours.Yellow, Color.FromArgb(0x66, 0xE8, 0xCB, 0x2D) },
        { Colours.Green, Color.FromArgb(0x66, 0x3D, 0xD4, 0x25) },
        { Colours.Teal, Color.FromArgb(0x66, 0x00, 0xD3, 0xB8) },
        { Colours.Blue, Color.FromArgb(0x66, 0x3D, 0xAE, 0xE9) },
        { Colours.Indigo, Color.FromArgb(0x66, 0x92, 0x6E, 0xE4) },
        { Colours.Violet, Color.FromArgb(0x66, 0xB8, 0x75, 0xDC) }
    };

    private static readonly Dictionary<Colours, Color> ThemeAccentColor4 = new Dictionary<Colours, Color>
    {
        { Colours.Pink, Color.FromArgb(0x33, 0xE9, 0x3A, 0x9A) },
        { Colours.Red, Color.FromArgb(0x33, 0xE9, 0x3D, 0x58) },
        { Colours.Orange, Color.FromArgb(0x33, 0xE9, 0x64, 0x3A) },
        { Colours.Yellow, Color.FromArgb(0x33, 0xE8, 0xCB, 0x2D) },
        { Colours.Green, Color.FromArgb(0x33, 0x3D, 0xD4, 0x25) },
        { Colours.Teal, Color.FromArgb(0x33, 0x00, 0xD3, 0xB8) },
        { Colours.Blue, Color.FromArgb(0x33, 0x3D, 0xAE, 0xE9) },
        { Colours.Indigo, Color.FromArgb(0x33, 0x92, 0x6E, 0xE4) },
        { Colours.Violet, Color.FromArgb(0x33, 0xB8, 0x75, 0xDC) }
    };

    private static readonly Dictionary<Colours, Color> HighlightColor = new Dictionary<Colours, Color>
    {
        { Colours.Pink, AdjustValue(ThemeAccentColor0[Colours.Pink], 0.5) },
        { Colours.Red, AdjustValue(ThemeAccentColor0[Colours.Red], 0.5) },
        { Colours.Orange, AdjustValue(ThemeAccentColor0[Colours.Orange], 0.5) },
        { Colours.Yellow, AdjustValue(ThemeAccentColor0[Colours.Yellow], 0.5) },
        { Colours.Green, AdjustValue(ThemeAccentColor0[Colours.Green], 0.5) },
        { Colours.Teal, AdjustValue(ThemeAccentColor0[Colours.Teal], 0.5) },
        { Colours.Blue, AdjustValue(ThemeAccentColor0[Colours.Blue], 0.5) },
        { Colours.Indigo, AdjustValue(ThemeAccentColor0[Colours.Indigo], 0.5) },
        { Colours.Violet, AdjustValue(ThemeAccentColor0[Colours.Violet], 0.5) },
    };

    private static readonly Dictionary<Colours, Color> HighlightColorDark = new Dictionary<Colours, Color>
    {
        { Colours.Pink, ThemeAccentColor0[Colours.Pink] },
        { Colours.Red, ThemeAccentColor0[Colours.Red] },
        { Colours.Orange, ThemeAccentColor0[Colours.Orange] },
        { Colours.Yellow, ThemeAccentColor0[Colours.Yellow] },
        { Colours.Green, ThemeAccentColor0[Colours.Green] },
        { Colours.Teal, ThemeAccentColor0[Colours.Teal] },
        { Colours.Blue, ThemeAccentColor0[Colours.Blue] },
        { Colours.Indigo, ThemeAccentColor0[Colours.Indigo] },
        { Colours.Violet, ThemeAccentColor0[Colours.Violet] }
    };

    //The background colour here is the ThemeBackgroundColour for light theme
    private static readonly Dictionary<Colours, Color> FocusedTextBoxTrackColor = new Dictionary<Colours, Color>
    {
        { Colours.Pink, CombineOverlayAndBase(ThemeAccentColor4[Colours.Pink], Colors.White) },
        { Colours.Red, CombineOverlayAndBase(ThemeAccentColor4[Colours.Red], Colors.White) },
        { Colours.Orange, CombineOverlayAndBase(ThemeAccentColor4[Colours.Orange], Colors.White) },
        { Colours.Yellow, CombineOverlayAndBase(ThemeAccentColor4[Colours.Yellow], Colors.White) },
        { Colours.Green, CombineOverlayAndBase(ThemeAccentColor4[Colours.Green], Colors.White) },
        { Colours.Teal, CombineOverlayAndBase(ThemeAccentColor4[Colours.Teal], Colors.White) },
        { Colours.Blue, CombineOverlayAndBase(ThemeAccentColor4[Colours.Blue], Colors.White) },
        { Colours.Indigo, CombineOverlayAndBase(ThemeAccentColor4[Colours.Indigo], Colors.White) },
        { Colours.Violet, CombineOverlayAndBase(ThemeAccentColor4[Colours.Violet], Colors.White) },
    };
    //The background colour here is the ThemeBackgroundColour for dark theme
    private static readonly Dictionary<Colours, Color> FocusedTextBoxTrackColorDark = new Dictionary<Colours, Color>
    {
        { Colours.Pink, CombineOverlayAndBase(ThemeAccentColor4[Colours.Pink], Color.FromRgb(0x0F, 0x0F, 0x0F)) },
        { Colours.Red, CombineOverlayAndBase(ThemeAccentColor4[Colours.Red], Color.FromRgb(0x0F, 0x0F, 0x0F)) },
        { Colours.Orange, CombineOverlayAndBase(ThemeAccentColor4[Colours.Orange], Color.FromRgb(0x0F, 0x0F, 0x0F)) },
        { Colours.Yellow, CombineOverlayAndBase(ThemeAccentColor4[Colours.Yellow], Color.FromRgb(0x0F, 0x0F, 0x0F)) },
        { Colours.Green, CombineOverlayAndBase(ThemeAccentColor4[Colours.Green], Color.FromRgb(0x0F, 0x0F, 0x0F)) },
        { Colours.Teal, CombineOverlayAndBase(ThemeAccentColor4[Colours.Teal], Color.FromRgb(0x0F, 0x0F, 0x0F)) },
        { Colours.Blue, CombineOverlayAndBase(ThemeAccentColor4[Colours.Blue], Color.FromRgb(0x0F, 0x0F, 0x0F)) },
        { Colours.Indigo, CombineOverlayAndBase(ThemeAccentColor4[Colours.Indigo], Color.FromRgb(0x0F, 0x0F, 0x0F)) },
        { Colours.Violet, CombineOverlayAndBase(ThemeAccentColor4[Colours.Violet], Color.FromRgb(0x0F, 0x0F, 0x0F)) }
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