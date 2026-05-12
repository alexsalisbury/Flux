namespace Flux.Tests;

using Flux.WinCore;

public class AppBarLayoutTests
{
    // --- ParseEdge ---

    [Theory]
    [InlineData("left", AppBarEdge.Left)]
    [InlineData("top", AppBarEdge.Top)]
    [InlineData("right", AppBarEdge.Right)]
    [InlineData("bottom", AppBarEdge.Bottom)]
    public void ParseEdge_ValidStrings(string input, AppBarEdge expected)
    {
        Assert.Equal(expected, AppBarLayout.ParseEdge(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("center")]
    [InlineData("TOP")]
    [InlineData("Left")]
    public void ParseEdge_InvalidOrUnrecognized_DefaultsToTop(string input)
    {
        Assert.Equal(AppBarEdge.Top, AppBarLayout.ParseEdge(input));
    }

    // --- ComputeInitialRect ---

    [Fact]
    public void InitialRect_Top_SpansFullWidth()
    {
        var rect = AppBarLayout.ComputeInitialRect(AppBarEdge.Top, 1920, 1080, 1.0);

        Assert.Equal(0, rect.Left);
        Assert.Equal(0, rect.Top);
        Assert.Equal(1920, rect.Right);
        Assert.Equal(0, rect.Bottom);
    }

    [Fact]
    public void InitialRect_Bottom_SpansFullWidth()
    {
        var rect = AppBarLayout.ComputeInitialRect(AppBarEdge.Bottom, 1920, 1080, 1.0);

        Assert.Equal(0, rect.Left);
        Assert.Equal(0, rect.Top);
        Assert.Equal(1920, rect.Right);
        Assert.Equal(0, rect.Bottom);
    }

    [Fact]
    public void InitialRect_Left_SpansFullHeight()
    {
        var rect = AppBarLayout.ComputeInitialRect(AppBarEdge.Left, 1920, 1080, 1.0);

        Assert.Equal(0, rect.Left);
        Assert.Equal(0, rect.Top);
        Assert.Equal(0, rect.Right);
        Assert.Equal(1080, rect.Bottom);
    }

    [Fact]
    public void InitialRect_Right_SpansFullHeight()
    {
        var rect = AppBarLayout.ComputeInitialRect(AppBarEdge.Right, 1920, 1080, 1.0);

        Assert.Equal(0, rect.Left);
        Assert.Equal(0, rect.Top);
        Assert.Equal(0, rect.Right);
        Assert.Equal(1080, rect.Bottom);
    }

    [Fact]
    public void InitialRect_WithScale_MultipliesDimensions()
    {
        var rect = AppBarLayout.ComputeInitialRect(AppBarEdge.Top, 1920, 1080, 1.5);

        Assert.Equal((int)(1920 * 1.5), rect.Right);
    }

    [Fact]
    public void InitialRect_LeftWithScale_MultipliesHeight()
    {
        var rect = AppBarLayout.ComputeInitialRect(AppBarEdge.Left, 1920, 1080, 2.0);

        Assert.Equal((int)(1080 * 2.0), rect.Bottom);
    }

    // --- ComputeAdjustedRect ---

    [Fact]
    public void AdjustedRect_Top_SetsBottom()
    {
        var initial = new AppBarRect(0, 0, 1920, 0);
        var adjusted = AppBarLayout.ComputeAdjustedRect(
            AppBarEdge.Top, initial, 1080, 1.0,
            barHeight: 40, marginYTop: 0, marginYBottom: 0);

        Assert.Equal(0, adjusted.Left);
        Assert.Equal(0, adjusted.Top);
        Assert.Equal(1920, adjusted.Right);
        Assert.Equal(40, adjusted.Bottom);
    }

    [Fact]
    public void AdjustedRect_Top_IncludesDoubleMargin()
    {
        var initial = new AppBarRect(0, 0, 1920, 0);
        var adjusted = AppBarLayout.ComputeAdjustedRect(
            AppBarEdge.Top, initial, 1080, 1.0,
            barHeight: 40, marginYTop: 5, marginYBottom: 0);

        Assert.Equal((int)((40 + 2 * 5) * 1.0), adjusted.Bottom);
    }

    [Fact]
    public void AdjustedRect_Top_WithScale()
    {
        var initial = new AppBarRect(0, 0, 2880, 0);
        var adjusted = AppBarLayout.ComputeAdjustedRect(
            AppBarEdge.Top, initial, 1080, 1.5,
            barHeight: 40, marginYTop: 0, marginYBottom: 0);

        Assert.Equal((int)(40 * 1.5), adjusted.Bottom);
    }

    [Fact]
    public void AdjustedRect_Bottom_SetsTopAndBottom()
    {
        var initial = new AppBarRect(0, 0, 1920, 0);
        var adjusted = AppBarLayout.ComputeAdjustedRect(
            AppBarEdge.Bottom, initial, 1080, 1.0,
            barHeight: 40, marginYTop: 0, marginYBottom: 0);

        Assert.Equal((int)((1080 - 40) * 1.0), adjusted.Top);
        Assert.Equal((int)(1080 * 1.0), adjusted.Bottom);
    }

    [Fact]
    public void AdjustedRect_Bottom_IncludesDoubleMargin()
    {
        var initial = new AppBarRect(0, 0, 1920, 0);
        var adjusted = AppBarLayout.ComputeAdjustedRect(
            AppBarEdge.Bottom, initial, 1080, 1.0,
            barHeight: 40, marginYTop: 0, marginYBottom: 10);

        Assert.Equal((int)((1080 - 40 - 2 * 10) * 1.0), adjusted.Top);
        Assert.Equal((int)(1080 * 1.0), adjusted.Bottom);
    }

    [Fact]
    public void AdjustedRect_Bottom_WithScale()
    {
        var initial = new AppBarRect(0, 0, 2880, 0);
        var adjusted = AppBarLayout.ComputeAdjustedRect(
            AppBarEdge.Bottom, initial, 1080, 1.5,
            barHeight: 40, marginYTop: 0, marginYBottom: 0);

        Assert.Equal((int)((1080 - 40) * 1.5), adjusted.Top);
        Assert.Equal((int)(1080 * 1.5), adjusted.Bottom);
    }

    [Fact]
    public void AdjustedRect_Left_ReturnsInitialUnchanged()
    {
        var initial = new AppBarRect(0, 0, 0, 1080);
        var adjusted = AppBarLayout.ComputeAdjustedRect(
            AppBarEdge.Left, initial, 1080, 1.0,
            barHeight: 40, marginYTop: 0, marginYBottom: 0);

        Assert.Equal(initial, adjusted);
    }

    [Fact]
    public void AdjustedRect_Right_ReturnsInitialUnchanged()
    {
        var initial = new AppBarRect(0, 0, 0, 1080);
        var adjusted = AppBarLayout.ComputeAdjustedRect(
            AppBarEdge.Right, initial, 1080, 1.0,
            barHeight: 40, marginYTop: 0, marginYBottom: 0);

        Assert.Equal(initial, adjusted);
    }

    // --- ComputeWindowTop ---

    [Fact]
    public void WindowTop_TopEdge_ReturnsMarginYTop()
    {
        var top = AppBarLayout.ComputeWindowTop("top", 1080, 40, marginYTop: 5, marginYBottom: 0);
        Assert.Equal(5, top);
    }

    [Fact]
    public void WindowTop_BottomEdge_PositionsFromBottom()
    {
        var top = AppBarLayout.ComputeWindowTop("bottom", 1080, 40, marginYTop: 0, marginYBottom: 10);
        Assert.Equal(1080 - (40 + 10), top);
    }

    [Fact]
    public void WindowTop_UnknownEdge_DefaultsToMarginYTop()
    {
        var top = AppBarLayout.ComputeWindowTop("left", 1080, 40, marginYTop: 8, marginYBottom: 0);
        Assert.Equal(8, top);
    }

    [Fact]
    public void WindowTop_TopEdge_ZeroMargin()
    {
        var top = AppBarLayout.ComputeWindowTop("top", 1080, 40, marginYTop: 0, marginYBottom: 0);
        Assert.Equal(0, top);
    }

    [Fact]
    public void WindowTop_BottomEdge_ZeroMargin()
    {
        var top = AppBarLayout.ComputeWindowTop("bottom", 1080, 40, marginYTop: 0, marginYBottom: 0);
        Assert.Equal(1040, top);
    }

    // --- AppBarRect record ---

    [Fact]
    public void AppBarRect_WithExpression()
    {
        var original = new AppBarRect(0, 0, 1920, 0);
        var modified = original with { Bottom = 40 };

        Assert.Equal(0, original.Bottom);
        Assert.Equal(40, modified.Bottom);
        Assert.Equal(1920, modified.Right);
    }

    [Fact]
    public void AppBarRect_Equality()
    {
        var a = new AppBarRect(0, 0, 1920, 1080);
        var b = new AppBarRect(0, 0, 1920, 1080);
        Assert.Equal(a, b);
    }

    // --- AppBarEdge enum ---

    [Theory]
    [InlineData(AppBarEdge.Left, 0)]
    [InlineData(AppBarEdge.Top, 1)]
    [InlineData(AppBarEdge.Right, 2)]
    [InlineData(AppBarEdge.Bottom, 3)]
    public void AppBarEdge_ValuesMatchWin32Constants(AppBarEdge edge, int expected)
    {
        Assert.Equal(expected, (int)edge);
    }

    // --- Real-world scenarios ---

    [Fact]
    public void Scenario_1080p_TopBar_NoMargins()
    {
        var edge = AppBarLayout.ParseEdge("top");
        var initial = AppBarLayout.ComputeInitialRect(edge, 1920, 1080, 1.0);
        var adjusted = AppBarLayout.ComputeAdjustedRect(edge, initial, 1080, 1.0, 40, 0, 0);

        Assert.Equal(new AppBarRect(0, 0, 1920, 40), adjusted);
    }

    [Fact]
    public void Scenario_4K_150Percent_BottomBar_WithMargins()
    {
        var edge = AppBarLayout.ParseEdge("bottom");
        var initial = AppBarLayout.ComputeInitialRect(edge, 2560, 1440, 1.5);
        var adjusted = AppBarLayout.ComputeAdjustedRect(edge, initial, 1440, 1.5, 40, 0, 10);

        Assert.Equal((int)((1440 - 40 - 20) * 1.5), adjusted.Top);
        Assert.Equal((int)(1440 * 1.5), adjusted.Bottom);
    }

    [Fact]
    public void Scenario_DefaultConfig_TopDocked()
    {
        var edge = AppBarLayout.ParseEdge("top");
        var initial = AppBarLayout.ComputeInitialRect(edge, 1920, 1080, 1.0);
        var adjusted = AppBarLayout.ComputeAdjustedRect(edge, initial, 1080, 1.0, 40, 0, 0);
        var windowTop = AppBarLayout.ComputeWindowTop("top", 1080, 40, 0, 0);

        Assert.Equal(0, windowTop);
        Assert.Equal(40, adjusted.Bottom);
    }
}
