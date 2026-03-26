namespace Flux.FluxWindows;

using Flux.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public sealed class TimelineWindow : Window
{
    public TimelineWindow(long resourceId, List<Revision> revisions, List<Act> acts)
    {
        Title = $"Timeline for Resource #{resourceId}";
        Width = 650;
        Height = 600;

        var list = new ListView
        {
            Margin = new Thickness(8),
            Background = new SolidColorBrush(Color.FromRgb(35, 35, 35)),
            Foreground = Brushes.White
        };

        foreach (var r in revisions)
        {
            var item = new ListBoxItem();
            var revActs = acts.Where(a => a.RevisionId == r.RevisionId).ToList();

            var label = $"{r.RevisionId} | {r.Kind} | {r.CreatedUtc:u}"
                      + (revActs.Count > 0
                         ? $" | Acts: {string.Join(", ", revActs.Select(a => a.Kind))}"
                         : "");

            item.Content = label;
            list.Items.Add(item);
        }

        Content = list;
    }
}
