namespace Flux.WinCore.Widgets;

using System.Windows;

public interface IWidgetManager
{
    void Hide(string key);
    void Show(string key);
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