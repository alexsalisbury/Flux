namespace Flux.Widgets;

using Flux.WinCore.Widgets;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

public sealed class SearchWidget : Widget
{
    public override string Key => "search";

    public event EventHandler<string>? SearchRequested;

    private readonly TextBox _box;
    private readonly Button _go;

    public SearchWidget()
    {
        // UI container for the widget
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        _box = new TextBox
        {
            Width = 300,
            Height = 28,
            Margin = new Thickness(0, 0, 4, 0),
            Foreground = Brushes.White,
            Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(6)
        };

        _go = new Button
        {
            Content = "🔍",
            Width = 32,
            Height = 28,
            Background = new SolidColorBrush(Color.FromRgb(0x00, 0xAD, 0xB5)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
        };

        panel.Children.Add(_box);
        panel.Children.Add(_go);

        this.Child = panel;
    }

    public override void Init()
    {
        _box.KeyDown += (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                Raise();
        };

        _go.Click += (_, __) => Raise();
    }

    private void Raise()
    {
        var text = _box.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(text))
            SearchRequested?.Invoke(this, text);
    }
}
