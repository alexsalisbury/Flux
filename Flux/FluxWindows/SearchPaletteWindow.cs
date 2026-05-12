namespace Flux.FluxWindows;

using Flux.Data;
using Flux.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

public sealed class SearchPaletteWindow : Window
{
    private readonly AppController _controller;
    private readonly TextBox _input;
    private readonly ListView _results;
    private ActFilter _act = ActFilter.CommitOnly;

    public event EventHandler<long>? OpenResourceRequested;

    public SearchPaletteWindow(AppController controller)
    {
        _controller = controller;

        Title = "Search";
        Width = 720;
        Height = 520;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = new SolidColorBrush(Color.FromRgb(0x23, 0x23, 0x23));

        var root = new DockPanel { Margin = new Thickness(8) };

        var bar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };

        _input = new TextBox
        {
            Width = 540,
            Height = 28,
            Margin = new Thickness(0, 0, 8, 0),
            Foreground = Brushes.White,
            Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(6)
        };

        var filter = new ComboBox
        {
            Width = 120,
            ItemsSource = new[] { "Committed", "Drafts", "Any" },
            SelectedIndex = 0
        };

        var go = new Button
        {
            Content = "Search",
            Height = 28,
            Padding = new Thickness(12, 4, 12, 4),
            Background = new SolidColorBrush(Color.FromRgb(0x00, 0xAD, 0xB5)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0)
        };

        bar.Children.Add(_input);
        bar.Children.Add(filter);
        bar.Children.Add(go);

        _results = new ListView
        {
            Margin = new Thickness(0, 6, 0, 0),
            Background = new SolidColorBrush(Color.FromRgb(35, 35, 35)),
            Foreground = Brushes.White
        };

        DockPanel.SetDock(bar, Dock.Top);
        root.Children.Add(bar);
        root.Children.Add(_results);
        Content = root;

        filter.SelectionChanged += (_, __) =>
        {
            _act = filter.SelectedIndex switch
            {
                1 => ActFilter.Drafts,
                2 => ActFilter.Any,
                _ => ActFilter.CommitOnly
            };
        };

        go.Click += async (_, __) => await RunAsync();
        _input.KeyDown += async (_, e) =>
        {
            if (e.Key == Key.Enter) { await RunAsync(); e.Handled = true; }
        };

        _results.MouseDoubleClick += (_, __) =>
        {
            if (_results.SelectedItem is Resource r)
            {
                OpenResourceRequested?.Invoke(this, r.ResourceId);
                Close();
            }
        };
        _results.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter && _results.SelectedItem is Resource r)
            {
                OpenResourceRequested?.Invoke(this, r.ResourceId);
                Close();
            }
        };
    }

    private async Task RunAsync()
    {
        var q = _input.Text?.Trim();
        if (string.IsNullOrWhiteSpace(q)) return;

        var rows = await _controller.SearchAsync(q, _act);
        _results.ItemsSource = rows
            .Select(r => new { r.ResourceId, r.Title, r.Kind, r.CreatedUtc })
            .ToList();
    }
}
