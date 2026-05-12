namespace Flux.Tests;

using Flux.WinCore;
using Flux.WinCore.Widgets;
using NSubstitute;

public class WidgetManagerTests
{
    private static (ILayout layout, WidgetManager mgr) Setup(params (string key, IWidget widget)[] widgets)
    {
        var layout = Substitute.For<ILayout>();
        var dict = widgets.ToDictionary(w => w.key, w => w.widget, StringComparer.OrdinalIgnoreCase);

        layout.AllWidgets.Returns(dict.Values);
        layout.TryGetWidget(Arg.Any<string>(), out Arg.Any<IWidget>())
              .Returns(ci =>
              {
                  var key = ci.ArgAt<string>(0);
                  if (dict.TryGetValue(key, out var w))
                  {
                      ci[1] = w;
                      return true;
                  }
                  ci[1] = null;
                  return false;
              });

        var mgr = new WidgetManager(layout);
        return (layout, mgr);
    }

    private static IActiveWidget MakeActiveWidget(bool isActive = false, bool isVisible = true)
    {
        var w = Substitute.For<IActiveWidget>();
        w.IsActive.Returns(isActive);
        w.IsVisible.Returns(isVisible);
        return w;
    }

    private static IWidget MakeWidget(bool isVisible = true)
    {
        var w = Substitute.For<IWidget>();
        w.IsVisible.Returns(isVisible);
        return w;
    }

    // --- Constructor ---

    [Fact]
    public void Constructor_SetsManagerOnLayout()
    {
        var layout = Substitute.For<ILayout>();
        var mgr = new WidgetManager(layout);
        Assert.Equal(mgr, layout.Manager);
    }

    [Fact]
    public void Constructor_NullLayout_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new WidgetManager(null!));
    }

    // --- StartAll ---

    [Fact]
    public void StartAll_CallsStartOnAllWidgets()
    {
        var w1 = MakeWidget();
        var w2 = MakeWidget();
        var (_, mgr) = Setup(("a", w1), ("b", w2));

        mgr.StartAll();

        w1.Received(1).Start();
        w2.Received(1).Start();
    }

    [Fact]
    public void StartAll_NoWidgets_DoesNotThrow()
    {
        var (_, mgr) = Setup();
        mgr.StartAll();
    }

    // --- StopAll ---

    [Fact]
    public void StopAll_CallsStopOnAllWidgets()
    {
        var w1 = MakeWidget();
        var w2 = MakeWidget();
        var (_, mgr) = Setup(("a", w1), ("b", w2));

        mgr.StopAll();

        w1.Received(1).Stop();
        w2.Received(1).Stop();
    }

    [Fact]
    public void StopAll_NoWidgets_DoesNotThrow()
    {
        var (_, mgr) = Setup();
        mgr.StopAll();
    }

    // --- TryGet ---

    [Fact]
    public void TryGet_ExistingWidget_ReturnsTrue()
    {
        var w = MakeActiveWidget();
        var (_, mgr) = Setup(("clock", w));

        var found = mgr.TryGet<IActiveWidget>("clock", out var result);

        Assert.True(found);
        Assert.Same(w, result);
    }

    [Fact]
    public void TryGet_MissingKey_ReturnsFalse()
    {
        var (_, mgr) = Setup();

        var found = mgr.TryGet<IWidget>("missing", out var result);

        Assert.False(found);
        Assert.Null(result);
    }

    [Fact]
    public void TryGet_WrongType_ReturnsFalse()
    {
        var w = MakeWidget();
        var (_, mgr) = Setup(("plain", w));

        var found = mgr.TryGet<IActiveWidget>("plain", out var result);

        Assert.False(found);
        Assert.Null(result);
    }

    // --- Show ---

    [Fact]
    public void Show_ExistingWidget_CallsShowAndReturnsTrue()
    {
        var w = MakeWidget();
        var (_, mgr) = Setup(("w1", w));

        var result = mgr.Show("w1");

        Assert.True(result);
        w.Received(1).Show();
    }

    [Fact]
    public void Show_MissingKey_ReturnsFalse()
    {
        var (_, mgr) = Setup();

        var result = mgr.Show("nope");

        Assert.False(result);
    }

    [Fact]
    public void Show_InactiveActiveWidget_StartsIt()
    {
        var w = MakeActiveWidget(isActive: false);
        var (_, mgr) = Setup(("w1", w));

        mgr.Show("w1");

        w.Received(1).Show();
        w.Received(1).Start();
    }

    [Fact]
    public void Show_AlreadyActiveWidget_DoesNotStartAgain()
    {
        var w = MakeActiveWidget(isActive: true);
        var (_, mgr) = Setup(("w1", w));

        mgr.Show("w1");

        w.Received(1).Show();
        w.DidNotReceive().Start();
    }

    [Fact]
    public void Show_NonActiveWidget_DoesNotCallStart()
    {
        var w = MakeWidget();
        var (_, mgr) = Setup(("w1", w));

        mgr.Show("w1");

        w.Received(1).Show();
        w.DidNotReceive().Start();
    }

    // --- Hide ---

    [Fact]
    public void Hide_ExistingWidget_CallsHideAndReturnsTrue()
    {
        var w = MakeWidget();
        var (_, mgr) = Setup(("w1", w));

        var result = mgr.Hide("w1");

        Assert.True(result);
        w.Received(1).Hide();
    }

    [Fact]
    public void Hide_MissingKey_ReturnsFalse()
    {
        var (_, mgr) = Setup();

        var result = mgr.Hide("nope");

        Assert.False(result);
    }

    [Fact]
    public void Hide_ActiveWidget_StopsIt()
    {
        var w = MakeActiveWidget(isActive: true);
        var (_, mgr) = Setup(("w1", w));

        mgr.Hide("w1");

        w.Received(1).Stop();
        w.Received(1).Hide();
    }

    [Fact]
    public void Hide_InactiveActiveWidget_DoesNotStop()
    {
        var w = MakeActiveWidget(isActive: false);
        var (_, mgr) = Setup(("w1", w));

        mgr.Hide("w1");

        w.DidNotReceive().Stop();
        w.Received(1).Hide();
    }

    [Fact]
    public void Hide_NonActiveWidget_DoesNotCallStop()
    {
        var w = MakeWidget();
        var (_, mgr) = Setup(("w1", w));

        mgr.Hide("w1");

        w.DidNotReceive().Stop();
        w.Received(1).Hide();
    }

    // --- Show/Hide ordering ---

    [Fact]
    public void Hide_StopsBeforeHiding()
    {
        var w = MakeActiveWidget(isActive: true);
        var order = new List<string>();
        w.When(x => x.Stop()).Do(_ => order.Add("stop"));
        w.When(x => x.Hide()).Do(_ => order.Add("hide"));
        var (_, mgr) = Setup(("w1", w));

        mgr.Hide("w1");

        Assert.Equal(["stop", "hide"], order);
    }

    [Fact]
    public void Show_ShowsBeforeStarting()
    {
        var w = MakeActiveWidget(isActive: false);
        var order = new List<string>();
        w.When(x => x.Show()).Do(_ => order.Add("show"));
        w.When(x => x.Start()).Do(_ => order.Add("start"));
        var (_, mgr) = Setup(("w1", w));

        mgr.Show("w1");

        Assert.Equal(["show", "start"], order);
    }
}
