namespace Flux.Widgets;

using Flux.WinCore.Widgets;
using MongoDB.Bson;
using MongoDB.Driver;
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
    private readonly AppServices _services;

    public MongoStatusWidget(AppServices services)
    {
        _services = services;

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

        this.Child = panel;   // attach UI
    }

    public override void Start()
    {
        base.Start();
        _ = CheckAsync();
    }

    private async Task CheckAsync()
    {
        try
        {
            var db = _services.Conn.Database;

            // Try a simple ping command
            var cmd = new JsonCommand<object>("{ ping: 1 }");
            await db.RunCommandAsync(cmd);

            // Try a read
            var collections = await db.ListCollectionNamesAsync();
            await collections.MoveNextAsync();

            // Try a small write (increment a counter)
            var counter = _services.Collections.Database
                .GetCollection<BsonDocument>("diagnostic");
            await counter.InsertOneAsync(new BsonDocument("ts", DateTime.UtcNow));

            // If we made it here: ✅ AUTH + READ + WRITE work
            SetStatusOk();
        }
        catch (MongoCommandException mce)
        {
            if (mce.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase))
            {
                SetStatusUnauthorized();
                return;
            }

            SetStatusError(mce.Message);
        }
        catch (Exception ex)
        {
            SetStatusError(ex.Message);
        }
    }

    private void SetStatusOk()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            dot.Fill = new SolidColorBrush(Color.FromRgb(0x3A, 0xD1, 0x73)); // green
            _label.Text = "Mongo OK";
            ToolTip = "Connected, authenticated, and writable.";
        });
    }

    private void SetStatusUnauthorized()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            dot.Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0xD0, 0x00)); // yellow
            _label.Text = "Mongo Auth?";
            ToolTip = "Connected, but authentication failed. Check username/password/authSource.";
        });
    }

    private void SetStatusError(string msg)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            dot.Fill = new SolidColorBrush(Color.FromRgb(0xE0, 0x3A, 0x3A)); // red
            _label.Text = "Mongo ERR";
            ToolTip = $"Connection failed: {msg}";
        });
    }
}