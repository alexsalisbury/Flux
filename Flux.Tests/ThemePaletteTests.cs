namespace Flux.Tests;

using Flux.WinCore;
using System.Windows.Media;

public class ThemePaletteTests
{
    // --- FromHex valid inputs ---

    [Fact]
    public void FromHex_StandardHex_ReturnsColor()
    {
        var c = ThemePalette.FromHex("#FF0000");
        Assert.Equal(255, c.R);
        Assert.Equal(0, c.G);
        Assert.Equal(0, c.B);
    }

    [Fact]
    public void FromHex_WithAlpha_ReturnsColor()
    {
        var c = ThemePalette.FromHex("#80FF0000");
        Assert.Equal(0x80, c.A);
        Assert.Equal(255, c.R);
    }

    [Fact]
    public void FromHex_White()
    {
        var c = ThemePalette.FromHex("#FFFFFF");
        Assert.Equal(255, c.R);
        Assert.Equal(255, c.G);
        Assert.Equal(255, c.B);
    }

    [Fact]
    public void FromHex_Black()
    {
        var c = ThemePalette.FromHex("#000000");
        Assert.Equal(0, c.R);
        Assert.Equal(0, c.G);
        Assert.Equal(0, c.B);
    }

    // --- FromHex fallbacks ---

    [Fact]
    public void FromHex_NullOrEmpty_ReturnsBlack()
    {
        var c1 = ThemePalette.FromHex("");
        var c2 = ThemePalette.FromHex(null!);

        Assert.Equal(ThemePalette.Black, c1);
        Assert.Equal(ThemePalette.Black, c2);
    }

    [Fact]
    public void FromHex_Whitespace_ReturnsBlack()
    {
        var c = ThemePalette.FromHex("   ");
        Assert.Equal(ThemePalette.Black, c);
    }

    [Fact]
    public void FromHex_Garbage_ReturnsBlack()
    {
        var c = ThemePalette.FromHex("not-a-color");
        Assert.Equal(ThemePalette.Black, c);
    }

    [Fact]
    public void FromHex_MissingHash_StillParses()
    {
        var c = ThemePalette.FromHex("FF0000");
        Assert.Equal(255, c.R);
        Assert.Equal(0, c.G);
        Assert.Equal(0, c.B);
    }

    // --- BrushFromHex ---

    [Fact]
    public void BrushFromHex_ReturnsBrushWithCorrectColor()
    {
        var brush = ThemePalette.BrushFromHex("#00FF00");
        Assert.Equal(0, brush.Color.R);
        Assert.Equal(255, brush.Color.G);
        Assert.Equal(0, brush.Color.B);
    }

    [Fact]
    public void BrushFromHex_Invalid_ReturnsBrushWithBlack()
    {
        var brush = ThemePalette.BrushFromHex("garbage");
        Assert.Equal(ThemePalette.Black, brush.Color);
    }

    // --- Static palette colors ---

    [Fact]
    public void Palette_NearBlack_IsDark()
    {
        Assert.Equal(0x12, ThemePalette.NearBlack.R);
        Assert.Equal(0x12, ThemePalette.NearBlack.G);
        Assert.Equal(0x12, ThemePalette.NearBlack.B);
    }

    [Fact]
    public void Palette_Charcoal()
    {
        Assert.Equal(0x2A, ThemePalette.Charcoal.R);
        Assert.Equal(0x2A, ThemePalette.Charcoal.G);
        Assert.Equal(0x2A, ThemePalette.Charcoal.B);
    }

    [Fact]
    public void Palette_BrushesAreFrozen()
    {
        Assert.True(ThemePalette.BrushBlack.IsFrozen);
        Assert.True(ThemePalette.BrushNearBlack.IsFrozen);
        Assert.True(ThemePalette.BrushGraphite.IsFrozen);
        Assert.True(ThemePalette.BrushCharcoal.IsFrozen);
        Assert.True(ThemePalette.BrushGray600.IsFrozen);
    }
}
