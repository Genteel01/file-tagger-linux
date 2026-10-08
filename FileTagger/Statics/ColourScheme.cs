using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;

namespace FileTagger.Statics;

public static class ColourScheme
{
    public enum Colours
    {
        System,
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

    private const Colours DefaultColour = Colours.Blue;
    private const double HighlightMultiplier = 0.5;
    private const byte AccentOpacity0 = 0xFF;
    private const byte AccentOpacity1 = 0xCC;
    private const byte AccentOpacity2 = 0x99;
    private const byte AccentOpacity3 = 0x66;
    private const byte AccentOpacity4 = 0x33;
    private static readonly Color LightBackgroundColour = Colors.White;
    private static readonly Color DarkBackgroundColour = Color.FromRgb(0x0F, 0x0F, 0x0F);

    private static readonly Dictionary<Colours, Color> ThemeAccentColor0 = new Dictionary<Colours, Color>
    {
        { Colours.Pink, Color.FromArgb(AccentOpacity0, 0xE9, 0x3A, 0x9A) },
        { Colours.Red, Color.FromArgb(AccentOpacity0, 0xE9, 0x3D, 0x58) },
        { Colours.Orange, Color.FromArgb(AccentOpacity0, 0xE9, 0x64, 0x3A) },
        { Colours.Yellow, Color.FromArgb(AccentOpacity0, 0xE8, 0xCB, 0x2D) },
        { Colours.Green, Color.FromArgb(AccentOpacity0, 0x3D, 0xD4, 0x25) },
        { Colours.Teal, Color.FromArgb(AccentOpacity0, 0x00, 0xD3, 0xB8) },
        { Colours.Blue, Color.FromArgb(AccentOpacity0, 0x3D, 0xAE, 0xE9) },
        { Colours.Indigo, Color.FromArgb(AccentOpacity0, 0x92, 0x6E, 0xE4) },
        { Colours.Violet, Color.FromArgb(AccentOpacity0, 0xB8, 0x75, 0xDC) }
    };

    private static readonly Dictionary<Colours, Color> ThemeAccentColor1 = new Dictionary<Colours, Color>
    {
        { Colours.Pink, Color.FromArgb(AccentOpacity1, 0xE9, 0x3A, 0x9A) },
        { Colours.Red, Color.FromArgb(AccentOpacity1, 0xE9, 0x3D, 0x58) },
        { Colours.Orange, Color.FromArgb(AccentOpacity1, 0xE9, 0x64, 0x3A) },
        { Colours.Yellow, Color.FromArgb(AccentOpacity1, 0xE8, 0xCB, 0x2D) },
        { Colours.Green, Color.FromArgb(AccentOpacity1, 0x3D, 0xD4, 0x25) },
        { Colours.Teal, Color.FromArgb(AccentOpacity1, 0x00, 0xD3, 0xB8) },
        { Colours.Blue, Color.FromArgb(AccentOpacity1, 0x3D, 0xAE, 0xE9) },
        { Colours.Indigo, Color.FromArgb(AccentOpacity1, 0x92, 0x6E, 0xE4) },
        { Colours.Violet, Color.FromArgb(AccentOpacity1, 0xB8, 0x75, 0xDC) }
    };

    private static readonly Dictionary<Colours, Color> ThemeAccentColor2 = new Dictionary<Colours, Color>
    {
        { Colours.Pink, Color.FromArgb(AccentOpacity2, 0xE9, 0x3A, 0x9A) },
        { Colours.Red, Color.FromArgb(AccentOpacity2, 0xE9, 0x3D, 0x58) },
        { Colours.Orange, Color.FromArgb(AccentOpacity2, 0xE9, 0x64, 0x3A) },
        { Colours.Yellow, Color.FromArgb(AccentOpacity2, 0xE8, 0xCB, 0x2D) },
        { Colours.Green, Color.FromArgb(AccentOpacity2, 0x3D, 0xD4, 0x25) },
        { Colours.Teal, Color.FromArgb(AccentOpacity2, 0x00, 0xD3, 0xB8) },
        { Colours.Blue, Color.FromArgb(AccentOpacity2, 0x3D, 0xAE, 0xE9) },
        { Colours.Indigo, Color.FromArgb(AccentOpacity2, 0x92, 0x6E, 0xE4) },
        { Colours.Violet, Color.FromArgb(AccentOpacity2, 0xB8, 0x75, 0xDC) }
    };

    private static readonly Dictionary<Colours, Color> ThemeAccentColor3 = new Dictionary<Colours, Color>
    {
        { Colours.Pink, Color.FromArgb(AccentOpacity3, 0xE9, 0x3A, 0x9A) },
        { Colours.Red, Color.FromArgb(AccentOpacity3, 0xE9, 0x3D, 0x58) },
        { Colours.Orange, Color.FromArgb(AccentOpacity3, 0xE9, 0x64, 0x3A) },
        { Colours.Yellow, Color.FromArgb(AccentOpacity3, 0xE8, 0xCB, 0x2D) },
        { Colours.Green, Color.FromArgb(AccentOpacity3, 0x3D, 0xD4, 0x25) },
        { Colours.Teal, Color.FromArgb(AccentOpacity3, 0x00, 0xD3, 0xB8) },
        { Colours.Blue, Color.FromArgb(AccentOpacity3, 0x3D, 0xAE, 0xE9) },
        { Colours.Indigo, Color.FromArgb(AccentOpacity3, 0x92, 0x6E, 0xE4) },
        { Colours.Violet, Color.FromArgb(AccentOpacity3, 0xB8, 0x75, 0xDC) }
    };

    private static readonly Dictionary<Colours, Color> ThemeAccentColor4 = new Dictionary<Colours, Color>
    {
        { Colours.Pink, Color.FromArgb(AccentOpacity4, 0xE9, 0x3A, 0x9A) },
        { Colours.Red, Color.FromArgb(AccentOpacity4, 0xE9, 0x3D, 0x58) },
        { Colours.Orange, Color.FromArgb(AccentOpacity4, 0xE9, 0x64, 0x3A) },
        { Colours.Yellow, Color.FromArgb(AccentOpacity4, 0xE8, 0xCB, 0x2D) },
        { Colours.Green, Color.FromArgb(AccentOpacity4, 0x3D, 0xD4, 0x25) },
        { Colours.Teal, Color.FromArgb(AccentOpacity4, 0x00, 0xD3, 0xB8) },
        { Colours.Blue, Color.FromArgb(AccentOpacity4, 0x3D, 0xAE, 0xE9) },
        { Colours.Indigo, Color.FromArgb(AccentOpacity4, 0x92, 0x6E, 0xE4) },
        { Colours.Violet, Color.FromArgb(AccentOpacity4, 0xB8, 0x75, 0xDC) }
    };

    private static readonly Dictionary<Colours, Color> HighlightColor = new Dictionary<Colours, Color>
    {
        { Colours.Pink, AdjustValue(ThemeAccentColor0[Colours.Pink], HighlightMultiplier) },
        { Colours.Red, AdjustValue(ThemeAccentColor0[Colours.Red], HighlightMultiplier) },
        { Colours.Orange, AdjustValue(ThemeAccentColor0[Colours.Orange], HighlightMultiplier) },
        { Colours.Yellow, AdjustValue(ThemeAccentColor0[Colours.Yellow], HighlightMultiplier) },
        { Colours.Green, AdjustValue(ThemeAccentColor0[Colours.Green], HighlightMultiplier) },
        { Colours.Teal, AdjustValue(ThemeAccentColor0[Colours.Teal], HighlightMultiplier) },
        { Colours.Blue, AdjustValue(ThemeAccentColor0[Colours.Blue], HighlightMultiplier) },
        { Colours.Indigo, AdjustValue(ThemeAccentColor0[Colours.Indigo], HighlightMultiplier) },
        { Colours.Violet, AdjustValue(ThemeAccentColor0[Colours.Violet], HighlightMultiplier) },
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
        { Colours.Pink, CombineOverlayAndBase(ThemeAccentColor4[Colours.Pink], LightBackgroundColour) },
        { Colours.Red, CombineOverlayAndBase(ThemeAccentColor4[Colours.Red], LightBackgroundColour) },
        { Colours.Orange, CombineOverlayAndBase(ThemeAccentColor4[Colours.Orange], LightBackgroundColour) },
        { Colours.Yellow, CombineOverlayAndBase(ThemeAccentColor4[Colours.Yellow], LightBackgroundColour) },
        { Colours.Green, CombineOverlayAndBase(ThemeAccentColor4[Colours.Green], LightBackgroundColour) },
        { Colours.Teal, CombineOverlayAndBase(ThemeAccentColor4[Colours.Teal], LightBackgroundColour) },
        { Colours.Blue, CombineOverlayAndBase(ThemeAccentColor4[Colours.Blue], LightBackgroundColour) },
        { Colours.Indigo, CombineOverlayAndBase(ThemeAccentColor4[Colours.Indigo], LightBackgroundColour) },
        { Colours.Violet, CombineOverlayAndBase(ThemeAccentColor4[Colours.Violet], LightBackgroundColour) },
    };
    //The background colour here is the ThemeBackgroundColour for dark theme
    private static readonly Dictionary<Colours, Color> FocusedTextBoxTrackColorDark = new Dictionary<Colours, Color>
    {
        { Colours.Pink, CombineOverlayAndBase(ThemeAccentColor4[Colours.Pink], DarkBackgroundColour) },
        { Colours.Red, CombineOverlayAndBase(ThemeAccentColor4[Colours.Red], DarkBackgroundColour) },
        { Colours.Orange, CombineOverlayAndBase(ThemeAccentColor4[Colours.Orange], DarkBackgroundColour) },
        { Colours.Yellow, CombineOverlayAndBase(ThemeAccentColor4[Colours.Yellow], DarkBackgroundColour) },
        { Colours.Green, CombineOverlayAndBase(ThemeAccentColor4[Colours.Green], DarkBackgroundColour) },
        { Colours.Teal, CombineOverlayAndBase(ThemeAccentColor4[Colours.Teal], DarkBackgroundColour) },
        { Colours.Blue, CombineOverlayAndBase(ThemeAccentColor4[Colours.Blue], DarkBackgroundColour) },
        { Colours.Indigo, CombineOverlayAndBase(ThemeAccentColor4[Colours.Indigo], DarkBackgroundColour) },
        { Colours.Violet, CombineOverlayAndBase(ThemeAccentColor4[Colours.Violet], DarkBackgroundColour) },
    };

    /// <summary>
    /// Sets all accent colour resources to the value for the given colour.
    /// If set to <see cref="Colours.System"/>, builds the correct values for the system accent colour.
    /// </summary>
    public static void ApplyColoursToApp(Application app, Colours colour)
    {
        Colours newColour = colour;
        //If colour is system, build the correct values for the system accent colour
        if (colour == Colours.System)
        {
            PlatformColorValues? systemColours = app.PlatformSettings?.GetColorValues();
            if (systemColours != null)
            {
                Color baseColour = systemColours.AccentColor1;
                ThemeAccentColor0[newColour] = new Color(AccentOpacity0, baseColour.R, baseColour.G, baseColour.B);
                ThemeAccentColor1[newColour] = new Color(AccentOpacity1, baseColour.R, baseColour.G, baseColour.B);
                ThemeAccentColor2[newColour] = new Color(AccentOpacity2, baseColour.R, baseColour.G, baseColour.B);
                ThemeAccentColor3[newColour] = new Color(AccentOpacity3, baseColour.R, baseColour.G, baseColour.B);
                ThemeAccentColor4[newColour] = new Color(AccentOpacity4, baseColour.R, baseColour.G, baseColour.B);
                HighlightColor[newColour] = AdjustValue(ThemeAccentColor0[newColour], HighlightMultiplier);
                HighlightColorDark[newColour] = ThemeAccentColor0[newColour];
                FocusedTextBoxTrackColor[newColour] = CombineOverlayAndBase(ThemeAccentColor4[newColour], LightBackgroundColour);
                FocusedTextBoxTrackColorDark[newColour] = CombineOverlayAndBase(ThemeAccentColor4[newColour], DarkBackgroundColour);
            }
            else
            {
                newColour = DefaultColour;
            }
        }
        app.Resources["ThemeAccentColor0"] = ThemeAccentColor0[newColour];
        app.Resources["ThemeAccentColor"] = ThemeAccentColor1[newColour];
        app.Resources["ThemeAccentColor2"] = ThemeAccentColor2[newColour];
        app.Resources["ThemeAccentColor3"] = ThemeAccentColor3[newColour];
        app.Resources["ThemeAccentColor4"] = ThemeAccentColor4[newColour];

        if (app.ActualThemeVariant == ThemeVariant.Dark)
        {
            app.Resources["HighlightColor"] = HighlightColorDark[newColour];
            app.Resources["FocusedTextBoxTrackColor"] = FocusedTextBoxTrackColorDark[newColour];
        }
        else
        {
            app.Resources["HighlightColor"] = HighlightColor[newColour];
            app.Resources["FocusedTextBoxTrackColor"] = FocusedTextBoxTrackColor[newColour];
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