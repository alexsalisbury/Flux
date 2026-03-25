namespace Flux.WinCore;

using System.Windows.Media;

public class ThemePalette
{
    public static readonly Color Black = FromHex("#000000");
    public static readonly Color NearBlack = FromHex("#121212");
    public static readonly Color Graphite = FromHex("#1E1E1E");
    public static readonly Color Charcoal = FromHex("#2A2A2A");
    public static readonly Color Gray600 = FromHex("#3A3A3A");

    public static readonly SolidColorBrush BrushBlack = Frozen(new SolidColorBrush(Black));
    public static readonly SolidColorBrush BrushNearBlack = Frozen(new SolidColorBrush(NearBlack));
    public static readonly SolidColorBrush BrushGraphite = Frozen(new SolidColorBrush(Graphite));
    public static readonly SolidColorBrush BrushCharcoal = Frozen(new SolidColorBrush(Charcoal));
    public static readonly SolidColorBrush BrushGray600 = Frozen(new SolidColorBrush(Gray600));

    /// <summary>Accepts #RGB, #RRGGBB, or #AARRGGBB.
    /// If the input is invalid, returns <see cref="Black"/> as a safe fallback instead of throwing.</summary>
    public static Color FromHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return Black;

        try
        {
            return (Color)ColorConverter.ConvertFromString(hex)!;
        }
        catch (FormatException)
        {
            // Try a simple normalization: ensure the string starts with '#'
            try
            {
                var s = hex.Trim();
                if (!s.StartsWith('#')) s = "#" + s;
                return (Color)ColorConverter.ConvertFromString(s)!;
            }
            catch
            {
                return Black; // safe default
            }
        }
        catch
        {
            return Black;
        }
    }

    public static SolidColorBrush BrushFromHex(string hex)
    {
        var c = FromHex(hex);
        return new SolidColorBrush(c); // caller freezes if desired (some remain mutable for dynamic themes)
    }

    private static SolidColorBrush Frozen(SolidColorBrush b) { b.Freeze(); return b; }
}
