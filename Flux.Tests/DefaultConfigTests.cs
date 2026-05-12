namespace Flux.Tests;

using Flux.WinCore;
using System.Windows;

public class DefaultConfigTests
{
    [Fact]
    public void Height_Is40()
    {
        var cfg = new DefaultConfig();
        Assert.Equal(40, cfg.Height);
    }

    [Fact]
    public void Width_DefaultsToZero()
    {
        var cfg = new DefaultConfig();
        Assert.Equal(0, cfg.Width);
    }

    [Fact]
    public void Width_IsMutable()
    {
        var cfg = new DefaultConfig();
        cfg.Width = 1920;
        Assert.Equal(1920, cfg.Width);
    }

    [Fact]
    public void DockEdge_IsTop()
    {
        var cfg = new DefaultConfig();
        Assert.Equal("top", cfg.DockEdge);
    }

    [Fact]
    public void Margins_AllZero()
    {
        var cfg = new DefaultConfig();
        Assert.Equal(0, cfg.MarginXLeft);
        Assert.Equal(0, cfg.MarginXRight);
        Assert.Equal(0, cfg.MarginYTop);
        Assert.Equal(0, cfg.MarginYBottom);
    }

    [Fact]
    public void BackgroundColor_IsDark()
    {
        var cfg = new DefaultConfig();
        Assert.Equal("#111111", cfg.BackgroundColor);
    }

    [Fact]
    public void BorderColor_IsGreen()
    {
        var cfg = new DefaultConfig();
        Assert.Equal("#00FF00", cfg.BorderColor);
    }

    [Fact]
    public void BorderThickness_IsZero()
    {
        var cfg = new DefaultConfig();
        Assert.Equal(new Thickness(0), cfg.BorderThickness);
    }

    [Fact]
    public void ImplementsIConfig()
    {
        IConfig cfg = new DefaultConfig();
        Assert.Equal(40, cfg.Height);
        Assert.Equal("top", cfg.DockEdge);
    }
}
