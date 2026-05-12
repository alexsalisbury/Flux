namespace Flux.WinCore.Widgets;

public class WidgetManager : IWidgetManager
{
    private readonly ILayout _layout;

    public WidgetManager(ILayout layout)
    {
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        _layout.Manager = this;
    }

    public void StartAll()
    {
        foreach (var w in _layout.AllWidgets)
        {
            w.Start();
        }
    }

    public void StopAll()
    {
        foreach (var w in _layout.AllWidgets.Reverse()) // reverse can help if dependencies exist
        {
            w.Stop();
        }
    }

    public bool TryGet<TWidget>(string key, out TWidget widget) where TWidget : class, IWidget
    {
        if (_layout.TryGetWidget(key, out var found))
        {
            widget = found as TWidget;
            return widget != null;
        }
        widget = null;
        return false;
    }
    public bool Show(string key)
    {
        if (!_layout.TryGetWidget(key, out var w)) return false;
        w.Show();
        if (w is IActiveWidget aw && !aw.IsActive) aw.Start();
        return true;
    }

    public bool Hide(string key)
    {
        if (!_layout.TryGetWidget(key, out var w)) return false;
        if (w is IActiveWidget aw && aw.IsActive) aw.Stop();
        w.Hide();
        return true;
    }

}