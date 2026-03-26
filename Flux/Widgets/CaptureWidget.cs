namespace Flux.Widgets;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Flux.WinCore.Widgets;

public sealed class CaptureWidget : Widget
{
    public override string Key => "capture";

    public event EventHandler? CaptureRequested;

    private readonly Button _btn;

    public CaptureWidget()
    {
        _btn = new Button
        {
            Content = "Capture",
            Height = 28,
            Margin = new Thickness(4, 0, 4, 0),
            Padding = new Thickness(12, 4, 12, 4),
            Background = new SolidColorBrush(Color.FromRgb(0x00, 0xAD, 0xB5)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0)
        };

        this.Child = _btn;
    }

    public override void Init()
    {
        _btn.Click += (_, __) => CaptureRequested?.Invoke(this, EventArgs.Empty);
    }
}
