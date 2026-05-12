namespace Flux.Widgets;

using Flux.Data;
using Flux.WinCore.Widgets;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

public sealed class MongoStatusWidget : Widget
{
    public override string Key => "mongo-status";

    private readonly Ellipse dot;
    private readonly TextBlock _label;
    private readonly MongoDiagnostic _diagnostic;

    public MongoStatusWidget(AppServices services)
    {
        _diagnostic = new MongoDiagnostic(services.Conn.Database);

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(6, 0, 6, 0)
        };

        dot = new Ellipse
        {
            Width = 12,
            Height = 12,
            Margin = new Thickness(0, 0, 6, 0),
            Fill = Brushes.Gray
        };

        _label = new TextBlock
        {
            Text = "Mongo…",
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };

        panel.Children.Add(dot);
        panel.Children.Add(_label);

        this.Child = panel;
    }

    public override void Start()
    {
        base.Start();
        _ = RunCheckAsync();
    }

    private async Task RunCheckAsync()
    {
        var result = await _diagnostic.CheckAsync();
        ApplyStatus(result);
    }

    internal void ApplyStatus(MongoDiagnosticResult result)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            switch (result.Status)
            {
                case MongoStatus.Ok:
                    dot.Fill = new SolidColorBrush(Color.FromRgb(0x3A, 0xD1, 0x73));
                    _label.Text = "Mongo OK";
                    ToolTip = "Connected, authenticated, and writable.";
                    break;
                case MongoStatus.Unauthorized:
                    dot.Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0xD0, 0x00));
                    _label.Text = "Mongo Auth?";
                    ToolTip = "Connected, but authentication failed. Check username/password/authSource.";
                    break;
                case MongoStatus.Error:
                    dot.Fill = new SolidColorBrush(Color.FromRgb(0xE0, 0x3A, 0x3A));
                    _label.Text = "Mongo ERR";
                    ToolTip = $"Connection failed: {result.Message}";
                    break;
            }
        });
    }
}
