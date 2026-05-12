namespace Flux.Tests;

using Flux.FluxWindows;
using Flux.WinCore;
using Flux.WinCore.Widgets;
using NSubstitute;
using System.Windows.Controls;

public class DualLayoutTests
{
    private static IWidget MakeWidget()
    {
        var w = Substitute.For<IWidget>();
        w.View.Returns(_ => new Border());
        return w;
    }

    private static void RunSta(Action action)
    {
        Exception? caught = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { caught = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (caught != null) throw caught;
    }

    // --- Register validation ---

    [Fact]
    public void Register_NullKey_Throws() => RunSta(() =>
    {
        var layout = new DualLayout();
        Assert.Throws<ArgumentException>(() => layout.Register(null!, MakeWidget(), LayoutZone.Left));
    });

    [Fact]
    public void Register_EmptyKey_Throws() => RunSta(() =>
    {
        var layout = new DualLayout();
        Assert.Throws<ArgumentException>(() => layout.Register("", MakeWidget(), LayoutZone.Left));
    });

    [Fact]
    public void Register_WhitespaceKey_Throws() => RunSta(() =>
    {
        var layout = new DualLayout();
        Assert.Throws<ArgumentException>(() => layout.Register("   ", MakeWidget(), LayoutZone.Left));
    });

    [Fact]
    public void Register_NullWidget_Throws() => RunSta(() =>
    {
        var layout = new DualLayout();
        Assert.Throws<ArgumentNullException>(() => layout.Register("key", null!, LayoutZone.Left));
    });

    [Fact]
    public void Register_DuplicateKey_Throws() => RunSta(() =>
    {
        var layout = new DualLayout();
        layout.Register("w1", MakeWidget(), LayoutZone.Left);
        Assert.Throws<InvalidOperationException>(() => layout.Register("w1", MakeWidget(), LayoutZone.Right));
    });

    [Fact]
    public void Register_DuplicateKey_CaseInsensitive() => RunSta(() =>
    {
        var layout = new DualLayout();
        layout.Register("Clock", MakeWidget(), LayoutZone.Left);
        Assert.Throws<InvalidOperationException>(() => layout.Register("clock", MakeWidget(), LayoutZone.Right));
    });

    [Fact]
    public void Register_InvalidZone_Throws() => RunSta(() =>
    {
        var layout = new DualLayout();
        Assert.Throws<ArgumentOutOfRangeException>(() => layout.Register("w1", MakeWidget(), (LayoutZone)99));
    });

    [Fact]
    public void Register_CallsInit() => RunSta(() =>
    {
        var layout = new DualLayout();
        var w = MakeWidget();
        layout.Register("w1", w, LayoutZone.Left);
        w.Received(1).Init();
    });

    // --- TryGetWidget ---

    [Fact]
    public void TryGetWidget_Registered_ReturnsTrue() => RunSta(() =>
    {
        var layout = new DualLayout();
        var w = MakeWidget();
        layout.Register("w1", w, LayoutZone.Left);
        Assert.True(layout.TryGetWidget("w1", out var found));
        Assert.Same(w, found);
    });

    [Fact]
    public void TryGetWidget_CaseInsensitive() => RunSta(() =>
    {
        var layout = new DualLayout();
        var w = MakeWidget();
        layout.Register("Clock", w, LayoutZone.Left);
        Assert.True(layout.TryGetWidget("clock", out var found));
        Assert.Same(w, found);
    });

    [Fact]
    public void TryGetWidget_NotRegistered_ReturnsFalse() => RunSta(() =>
    {
        var layout = new DualLayout();
        Assert.False(layout.TryGetWidget("missing", out _));
    });

    // --- AllWidgets ---

    [Fact]
    public void AllWidgets_ReturnsAllRegistered() => RunSta(() =>
    {
        var layout = new DualLayout();
        var w1 = MakeWidget();
        var w2 = MakeWidget();
        layout.Register("a", w1, LayoutZone.Left);
        layout.Register("b", w2, LayoutZone.Right);
        var all = layout.AllWidgets.ToList();
        Assert.Equal(2, all.Count);
        Assert.Contains(w1, all);
        Assert.Contains(w2, all);
    });

    [Fact]
    public void AllWidgets_Empty_ReturnsEmpty() => RunSta(() =>
    {
        var layout = new DualLayout();
        Assert.Empty(layout.AllWidgets);
    });

    // --- Show/Hide delegate to Manager ---

    [Fact]
    public void Show_NoManager_ReturnsFalse() => RunSta(() =>
    {
        var layout = new DualLayout();
        Assert.False(layout.Show("w1"));
    });

    [Fact]
    public void Hide_NoManager_ReturnsFalse() => RunSta(() =>
    {
        var layout = new DualLayout();
        Assert.False(layout.Hide("w1"));
    });

    [Fact]
    public void Show_DelegatesToManager() => RunSta(() =>
    {
        var layout = new DualLayout();
        var mgr = Substitute.For<IWidgetManager>();
        mgr.Show("w1").Returns(true);
        layout.Manager = mgr;
        Assert.True(layout.Show("w1"));
        mgr.Received(1).Show("w1");
    });

    [Fact]
    public void Hide_DelegatesToManager() => RunSta(() =>
    {
        var layout = new DualLayout();
        var mgr = Substitute.For<IWidgetManager>();
        mgr.Hide("w1").Returns(true);
        layout.Manager = mgr;
        Assert.True(layout.Hide("w1"));
        mgr.Received(1).Hide("w1");
    });

    // --- Container ---

    [Fact]
    public void Container_IsNotNull() => RunSta(() =>
    {
        var layout = new DualLayout();
        Assert.NotNull(layout.Container);
    });

    // --- LayoutZone enum ---

    [Theory]
    [InlineData(LayoutZone.Left, 0)]
    [InlineData(LayoutZone.Right, 1)]
    public void LayoutZone_Values(LayoutZone zone, int expected)
    {
        Assert.Equal(expected, (int)zone);
    }
}
