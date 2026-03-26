namespace Flux.WinCore.Widgets;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public interface IWidgetManager
{
    bool Hide(string key);
    bool Show(string key);
    void StartAll();
    void StopAll();
}

public interface IWidget
{
    UIElement View { get; }         // The visual to insert into the layout
    void Init();                    // One-time setup
    void Start();                   // Start live behavior (timers, subscriptions)
    void Stop();                    // Stop live behavior
}

public interface IActiveWidget : IWidget
{
    bool IsActive { get; }
}

public abstract class Widget : Border, IActiveWidget, IDisposable
{
    public abstract string Key { get; }

    public UIElement View => this;

    public bool IsActive { get; private set; }

    public virtual void Init() { }

    public virtual void Start() => IsActive = true;

    public virtual void Stop() => IsActive = false;

    protected virtual void Dispose(bool disposing) { }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected Widget()
    {
        Background = Brushes.Transparent;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Stretch;
        Visibility = Visibility.Visible;
    }
}
