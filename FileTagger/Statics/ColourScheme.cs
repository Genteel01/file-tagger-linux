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

    private enum AccentOpacity : byte
    {
        Accent0 = 0xFF,
        Accent1 = 0xCC,
        Accent2 = 0x99,
        Accent3 = 0x66,
        Accent4 = 0x33,
    }

    /// <summary>
    /// List of <see cref="Colours"/> that can be selected
    /// </summary>
    public static Colours[] SelectableColours = [
        Colours.System,
        Colours.Red,
        Colours.Orange,
        Colours.Yellow,
        Colours.Green,
        Colours.Blue,
        Colours.Indigo,
        Colours.Violet];

    private const Colours DefaultColour = Colours.Blue;
    private const double HighlightMultiplier = 0.5;

    //ThemeBackgroundColour for light theme
    private static readonly Color LightBackgroundColour = Colors.White;
    //ThemeBackgroundColour for dark theme
    private static readonly Color DarkBackgroundColour = Color.FromRgb(0x0F, 0x0F, 0x0F);

    private static readonly Dictionary<Colours, (byte r, byte g, byte b)> AccentColours = new Dictionary<Colours, (byte r, byte g, byte b)>
    {
        { Colours.Pink, (0xE9, 0x3A, 0x9A) },
        { Colours.Red, (0xE9, 0x3D, 0x58) },
        { Colours.Orange, (0xE9, 0x64, 0x3A) },
        { Colours.Yellow, (0xE8, 0xCB, 0x2D) },
        { Colours.Green, (0x3D, 0xD4, 0x25) },
        { Colours.Teal, (0x00, 0xD3, 0xB8) },
        { Colours.Blue, (0x3D, 0xAE, 0xE9) },
        { Colours.Indigo, (0x92, 0x6E, 0xE4) },
        { Colours.Violet, (0xB8, 0x75, 0xDC) },
    };

    private static Color ThemeAccentColor(AccentOpacity opacity, Colours colour)
    {
        return Color.FromArgb((byte)opacity, AccentColours[colour].r, AccentColours[colour].g, AccentColours[colour].b);
    }

    private static Color HighlightColor(Colours colour, ThemeVariant theme)
    {
        if (theme == ThemeVariant.Dark) return ThemeAccentColor(AccentOpacity.Accent0, colour);
        return AdjustValue(ThemeAccentColor(AccentOpacity.Accent0, colour), HighlightMultiplier);
    }

    public static Color FocusedTextBoxTrackColor(Colours colour, ThemeVariant theme)
    {
        Color backgroundColour = theme == ThemeVariant.Dark ? DarkBackgroundColour : LightBackgroundColour;
        return CombineOverlayAndBase(ThemeAccentColor(AccentOpacity.Accent4, colour), backgroundColour);
    }

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
                BuildSystemColours(systemColours);
            }
            else
            {
                newColour = DefaultColour;
            }
        }
        app.Resources["ThemeAccentColor0"] = ThemeAccentColor(AccentOpacity.Accent0, newColour);
        app.Resources["ThemeAccentColor"] = ThemeAccentColor(AccentOpacity.Accent1, newColour);
        app.Resources["ThemeAccentColor2"] = ThemeAccentColor(AccentOpacity.Accent2, newColour);
        app.Resources["ThemeAccentColor3"] = ThemeAccentColor(AccentOpacity.Accent3, newColour);
        app.Resources["ThemeAccentColor4"] = ThemeAccentColor(AccentOpacity.Accent4, newColour);

        ApplyThemeColoursToApp(app, colour);
    }

    /// <summary>
    /// Applies the colours that are dependent on the <see cref="ThemeVariant"/>
    /// </summary>
    public static void ApplyThemeColoursToApp(Application app, Colours colour)
    {
        //If colour is System and we haven't built the colours for it, don't continue. Occurs once on startup via
        //app.ActualThemeVariantChanged if RequestedThemeVariant is 'default', system theme is dark, and the requested
        //colour is 'System'. Immediately afterwards it runs ApplyColoursToApp via app.PlatformSettings?.ColorValuesChanged,
        //which builds the system colours and runs ApplyThemeColoursToApp again.
        if (colour == Colours.System && !AccentColours.ContainsKey(colour))
        {
            return;
        }

        app.Resources["HighlightColor"] = HighlightColor(colour, app.ActualThemeVariant);
        app.Resources["FocusedTextBoxTrackColor"] = FocusedTextBoxTrackColor(colour, app.ActualThemeVariant);
    }

    private static void BuildSystemColours(PlatformColorValues systemColours)
    {
        Color baseColour = systemColours.AccentColor1;
        AccentColours[Colours.System] = (baseColour.R, baseColour.G, baseColour.B);
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