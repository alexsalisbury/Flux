namespace Flux;

using Flux.WinCore;
using Flux.WinCore.Widgets;
using System.Windows;
using System.Windows.Controls;

public class DualLayout : ILayout
{
    private readonly StackPanel _left = new() { Orientation = Orientation.Horizontal };
    private readonly StackPanel _right = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
    private readonly Dictionary<string, IWidget> _widgets = new(StringComparer.OrdinalIgnoreCase);

    public Panel Container { get; }

    public IEnumerable<IWidget> AllWidgets => _widgets.Values;

    public IWidgetManager Manager { get; set; }

    public DualLayout(double height = 20)
    {
        var root = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            Height = height
        };

        Grid.SetColumn(_left, 0);
        Grid.SetColumn(_right, 2);
        root.Children.Add(_left);
        root.Children.Add(_right);

        Container = root;
    }

    public void Register(string key, IWidget widget, LayoutZone zone)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key required", nameof(key));
        if (widget is null) throw new ArgumentNullException(nameof(widget));
        if (_widgets.ContainsKey(key)) throw new InvalidOperationException($"Widget '{key}' already registered.");

        _widgets[key] = widget;

        switch (zone)
        {
            case LayoutZone.Left:
                _left.Children.Add(widget.View);
                break;
            case LayoutZone.Right:
                _right.Children.Add(widget.View);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(zone));
        }

        widget.Init();
    }

    public bool TryGetWidget(string key, out IWidget widget) => _widgets.TryGetValue(key, out widget);

    public bool Hide(string key) => Manager?.Hide(key) ?? false;

    public bool Show(string key) => Manager?.Show(key) ?? false;
}