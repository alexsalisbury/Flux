namespace Flux.WinCore;

using Flux.WinCore.Widgets;
using System.Windows.Controls;

public enum LayoutZone { Left, Right }


public interface ILayout
{
    Panel Container { get; }        // Root container to host in TopBar.Content
    void Register(string key, IWidget widget, LayoutZone zone);
    bool TryGetWidget(string key, out IWidget widget);
    IEnumerable<IWidget> AllWidgets { get; }
    IWidgetManager Manager { get; set; }
    bool Show(string key);
    bool Hide(string key);
}