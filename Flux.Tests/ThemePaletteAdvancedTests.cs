namespace Flux.Tests;

using Flux.WinCore;
using System.Windows.Media;

public class ThemePaletteAdvancedTests
{
    // --- Named colors via ColorConverter ---

    [Fact]
    public void FromHex_NamedColor_Red()
    {
        var c = ThemePalette.FromHex("Red");
        Assert.Equal(255, c.R);
        Assert.Equal(0, c.G);
        Assert.Equal(0, c.B);
    }

    [Fact]
    public void FromHex_NamedColor_Transparent()
    {
        var c = ThemePalette.FromHex("Transparent");
        Assert.Equal(0, c.A);
    }

    // --- Hex format variations ---

    [Fact]
    public void FromHex_ShortHex_ThreeDigit()
    {
        var c = ThemePalette.FromHex("#F00");
        Assert.Equal(255, c.R);
        Assert.Equal(0, c.G);
        Assert.Equal(0, c.B);
    }

    [Fact]
    public void FromHex_Lowercase()
    {
        var c = ThemePalette.FromHex("#ff0000");
        Assert.Equal(255, c.R);
    }

    [Fact]
    public void FromHex_MixedCase()
    {
        var c = ThemePalette.FromHex("#Ff00fF");
        Assert.Equal(255, c.R);
        Assert.Equal(0, c.G);
        Assert.Equal(255, c.B);
    }

    [Fact]
    public void FromHex_WithLeadingWhitespace_StillParses()
    {
        var c = ThemePalette.FromHex("  #00FF00  ");
        Assert.Equal(0, c.R);
        Assert.Equal(255, c.G);
        Assert.Equal(0, c.B);
    }

    // --- Specific palette values used in production ---

    [Fact]
    public void Palette_Graphite()
    {
        Assert.Equal(0x1E, ThemePalette.Graphite.R);
        Assert.Equal(0x1E, ThemePalette.Graphite.G);
        Assert.Equal(0x1E, ThemePalette.Graphite.B);
    }

    [Fact]
    public void Palette_Gray600()
    {
        Assert.Equal(0x3A, ThemePalette.Gray600.R);
        Assert.Equal(0x3A, ThemePalette.Gray600.G);
        Assert.Equal(0x3A, ThemePalette.Gray600.B);
    }

    [Fact]
    public void Palette_Black_IsAllZero()
    {
        Assert.Equal(0, ThemePalette.Black.R);
        Assert.Equal(0, ThemePalette.Black.G);
        Assert.Equal(0, ThemePalette.Black.B);
        Assert.Equal(255, ThemePalette.Black.A);
    }

    // --- BrushFromHex is not frozen ---

    [Fact]
    public void BrushFromHex_IsNotFrozen()
    {
        var brush = ThemePalette.BrushFromHex("#FF0000");
        Assert.False(brush.IsFrozen);
    }

    // --- Edge cases ---

    [Fact]
    public void FromHex_HashOnly_ReturnsBlack()
    {
        var c = ThemePalette.FromHex("#");
        Assert.Equal(ThemePalette.Black, c);
    }

    [Fact]
    public void FromHex_DoubleHash_ReturnsBlack()
    {
        var c = ThemePalette.FromHex("##FF0000");
        Assert.Equal(ThemePalette.Black, c);
    }

    [Fact]
    public void FromHex_PartialHex_ReturnsBlack()
    {
        var c = ThemePalette.FromHex("#GG");
        Assert.Equal(ThemePalette.Black, c);
    }

    // --- Full alpha ---

    [Fact]
    public void FromHex_FullAlpha_OpaqueRed()
    {
        var c = ThemePalette.FromHex("#FFFF0000");
        Assert.Equal(255, c.A);
        Assert.Equal(255, c.R);
        Assert.Equal(0, c.G);
        Assert.Equal(0, c.B);
    }

    [Fact]
    public void FromHex_ZeroAlpha_TransparentGreen()
    {
        var c = ThemePalette.FromHex("#0000FF00");
        Assert.Equal(0, c.A);
        Assert.Equal(0, c.R);
        Assert.Equal(255, c.G);
        Assert.Equal(0, c.B);
    }

    // --- Brush colors match palette ---

    [Fact]
    public void BrushBlack_ColorMatchesBlack()
    {
        Assert.Equal(ThemePalette.Black, ThemePalette.BrushBlack.Color);
    }

    [Fact]
    public void BrushNearBlack_ColorMatchesNearBlack()
    {
        Assert.Equal(ThemePalette.NearBlack, ThemePalette.BrushNearBlack.Color);
    }

    [Fact]
    public void BrushGraphite_ColorMatchesGraphite()
    {
        Assert.Equal(ThemePalette.Graphite, ThemePalette.BrushGraphite.Color);
    }

    [Fact]
    public void BrushCharcoal_ColorMatchesCharcoal()
    {
        Assert.Equal(ThemePalette.Charcoal, ThemePalette.BrushCharcoal.Color);
    }

    [Fact]
    public void BrushGray600_ColorMatchesGray600()
    {
        Assert.Equal(ThemePalette.Gray600, ThemePalette.BrushGray600.Color);
    }
}
