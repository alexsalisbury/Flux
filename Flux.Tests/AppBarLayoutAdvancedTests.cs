namespace Flux.Tests;

using Flux.WinCore;

public class AppBarLayoutAdvancedTests
{
    // --- ParseEdge boundary ---

    [Theory]
    [InlineData("LEFT")]
    [InlineData("Top")]
    [InlineData("RIGHT")]
    [InlineData("Bottom")]
    [InlineData("TOP ")]
    [InlineData(" top")]
    public void ParseEdge_CaseSensitive_DefaultsToTop(string input)
    {
        Assert.Equal(AppBarEdge.Top, AppBarLayout.ParseEdge(input));
    }

    [Fact]
    public void ParseEdge_NullInput_DefaultsToTop()
    {
        Assert.Equal(AppBarEdge.Top, AppBarLayout.ParseEdge(null!));
    }

    // --- ComputeInitialRect with various scales ---

    [Theory]
    [InlineData(1.0)]
    [InlineData(1.25)]
    [InlineData(1.5)]
    [InlineData(2.0)]
    [InlineData(3.0)]
    public void InitialRect_Top_WidthScalesCorrectly(double scale)
    {
        var rect = AppBarLayout.ComputeInitialRect(AppBarEdge.Top, 1920, 1080, scale);
        Assert.Equal((int)(1920 * scale), rect.Right);
        Assert.Equal(0, rect.Bottom);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(1.25)]
    [InlineData(1.5)]
    [InlineData(2.0)]
    public void InitialRect_Left_HeightScalesCorrectly(double scale)
    {
        var rect = AppBarLayout.ComputeInitialRect(AppBarEdge.Left, 1920, 1080, scale);
        Assert.Equal((int)(1080 * scale), rect.Bottom);
        Assert.Equal(0, rect.Right);
    }

    // --- ComputeAdjustedRect with margins ---

    [Fact]
    public void AdjustedRect_Top_BothMarginsZero()
    {
        var initial = new AppBarRect(0, 0, 1920, 0);
        var adjusted = AppBarLayout.ComputeAdjustedRect(
            AppBarEdge.Top, initial, 1080, 1.0, 40, 0, 0);

        Assert.Equal(40, adjusted.Bottom);
    }

    [Fact]
    public void AdjustedRect_Top_LargeMargin()
    {
        var initial = new AppBarRect(0, 0, 1920, 0);
        var adjusted = AppBarLayout.ComputeAdjustedRect(
            AppBarEdge.Top, initial, 1080, 1.0, 40, 20, 0);

        Assert.Equal(40 + 2 * 20, adjusted.Bottom);
    }

    [Fact]
    public void AdjustedRect_Bottom_LargeMargin()
    {
        var initial = new AppBarRect(0, 0, 1920, 0);
        var adjusted = AppBarLayout.ComputeAdjustedRect(
            AppBarEdge.Bottom, initial, 1080, 1.0, 40, 0, 20);

        Assert.Equal((int)((1080 - 40 - 2 * 20) * 1.0), adjusted.Top);
    }

    // --- ComputeWindowTop edge cases ---

    [Fact]
    public void WindowTop_TopEdge_LargeMargin()
    {
        var top = AppBarLayout.ComputeWindowTop("top", 1080, 40, 100, 0);
        Assert.Equal(100, top);
    }

    [Fact]
    public void WindowTop_BottomEdge_LargeMargin()
    {
        var top = AppBarLayout.ComputeWindowTop("bottom", 1080, 40, 0, 100);
        Assert.Equal(1080 - (40 + 100), top);
    }

    [Fact]
    public void WindowTop_BottomEdge_BarHeightEqualsScreen()
    {
        var top = AppBarLayout.ComputeWindowTop("bottom", 1080, 1080, 0, 0);
        Assert.Equal(0, top);
    }

    // --- Symmetry tests ---

    [Fact]
    public void InitialRect_LeftAndRight_SameShape()
    {
        var left = AppBarLayout.ComputeInitialRect(AppBarEdge.Left, 1920, 1080, 1.0);
        var right = AppBarLayout.ComputeInitialRect(AppBarEdge.Right, 1920, 1080, 1.0);
        Assert.Equal(left, right);
    }

    [Fact]
    public void InitialRect_TopAndBottom_SameShape()
    {
        var top = AppBarLayout.ComputeInitialRect(AppBarEdge.Top, 1920, 1080, 1.0);
        var bottom = AppBarLayout.ComputeInitialRect(AppBarEdge.Bottom, 1920, 1080, 1.0);
        Assert.Equal(top, bottom);
    }

    // --- Extreme values ---

    [Fact]
    public void InitialRect_ZeroDimensions()
    {
        var rect = AppBarLayout.ComputeInitialRect(AppBarEdge.Top, 0, 0, 1.0);
        Assert.Equal(new AppBarRect(0, 0, 0, 0), rect);
    }

    [Fact]
    public void AdjustedRect_ZeroBarHeight()
    {
        var initial = new AppBarRect(0, 0, 1920, 0);
        var adjusted = AppBarLayout.ComputeAdjustedRect(
            AppBarEdge.Top, initial, 1080, 1.0, 0, 0, 0);

        Assert.Equal(0, adjusted.Bottom);
    }

    [Fact]
    public void WindowTop_ZeroBarHeight_Top()
    {
        var top = AppBarLayout.ComputeWindowTop("top", 1080, 0, 0, 0);
        Assert.Equal(0, top);
    }

    [Fact]
    public void WindowTop_ZeroBarHeight_Bottom()
    {
        var top = AppBarLayout.ComputeWindowTop("bottom", 1080, 0, 0, 0);
        Assert.Equal(1080, top);
    }
}
