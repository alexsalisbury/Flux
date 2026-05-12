using Flux;
using Flux.FluxWindows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

public sealed class ResourceEditorWindow : Window
{
    private readonly AppController _controller;
    private readonly long _resourceId;

    private readonly TextBox _editor;
    private readonly Button _saveDraft;
    private readonly Button _commit;
    private readonly Button _timeline; 

    // Let AppBar (or others) know which resource is active?
    public event EventHandler<long>? ResourceActivated;

    public ResourceEditorWindow(AppController controller, long resourceId, string? initialBody)
    {
        _controller = controller;
        _resourceId = resourceId;

        Title = $"Edit Resource #{resourceId}";
        Width = 760;
        Height = 540;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = new SolidColorBrush(Color.FromRgb(0x23, 0x23, 0x23));

        var root = new DockPanel { Margin = new Thickness(8) };

        // Toolbar
        var bar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 6),
            VerticalAlignment = VerticalAlignment.Center
        };

        _saveDraft = new Button
        {
            Content = "Save Draft",
            Margin = new Thickness(0, 0, 8, 0),
            Padding = new Thickness(12, 4, 12, 4)
        };

        _commit = new Button
        {
            Content = "Commit",
            Margin = new Thickness(0, 0, 8, 0),
            Padding = new Thickness(12, 4, 12, 4)
        };

        _timeline = new Button 
        {
            Content = "Timeline",
            Padding = new Thickness(12, 4, 12, 4)
        };

        bar.Children.Add(_saveDraft);
        bar.Children.Add(_commit);
        bar.Children.Add(_timeline);

        // Editor
        _editor = new TextBox
        {
            AcceptsReturn = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            TextWrapping = TextWrapping.Wrap,
            Text = initialBody ?? "",
            FontFamily = new FontFamily("Consolas"),
            FontSize = 14,
            Foreground = Brushes.White,
            Background = new SolidColorBrush(Color.FromRgb(0x2B, 0x2B, 0x2B)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
            BorderThickness = new Thickness(1)
        };

        // Layout
        DockPanel.SetDock(bar, Dock.Top);
        root.Children.Add(bar);
        root.Children.Add(_editor);

        Content = root;

        // Events
        _saveDraft.Click += async (_, __) =>
        {
            await _controller.SaveDraftAsync(_resourceId, _editor.Text);
            await _controller.WeaveAsync(_resourceId, _editor.Text, createMissing: false); // resolve only
            MessageBox.Show("Draft saved + woven.");
        };

        _commit.Click += async (_, __) =>
        {
            await _controller.CommitAsync(_resourceId, _editor.Text);
            await _controller.WeaveAsync(_resourceId, _editor.Text, createMissing: true); // create missing on commit
            MessageBox.Show("Committed + woven.");
        };

        _timeline.Click += async (_, __) => await OpenTimelineAsync();

        PreviewKeyDown += async (_, e) =>
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                await _controller.SaveDraftAsync(_resourceId, _editor.Text);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                await _controller.CommitAsync(_resourceId, _editor.Text);
                e.Handled = true;
            }
            else if (e.Key == Key.T && Keyboard.Modifiers == ModifierKeys.Control) 
            {
                await OpenTimelineAsync();
                e.Handled = true;
            }
        };

        ResourceActivated?.Invoke(this, _resourceId);
        Activated += (_, __) => ResourceActivated?.Invoke(this, _resourceId);
    }

    private async Task OpenTimelineAsync()
    {
        var (revs, acts) = await _controller.LoadTimelineAsync(_resourceId);
        new TimelineWindow(_resourceId, revs, acts).Show();
    }
}