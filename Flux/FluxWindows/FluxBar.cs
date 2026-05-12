namespace Flux.FluxWindows;

using Flux.Widgets;
using Flux.WinCore;
using Flux.WinCore.SysInfra;
using Microsoft.Extensions.Configuration;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

public sealed class FluxBar : TopBar
{
    private readonly AppServices services;
    private readonly AppController controller;

    private Application currentApp;
    private Hotkey? hotkey;

    public FluxBar(IConfigurationRoot configuration, IConfig cfg, Application current) : base(cfg)
    {
        currentApp = current;

        services = new AppServices(configuration);
        controller = new AppController(services);

        this.Loaded += (_, __) =>
        {
            this.hotkey = new Hotkey(this, ModifierKeys.Control | ModifierKeys.Shift, Key.Space, () =>
            {
                //TODO:
            });
        };
    }

    public void Initialize(Dispatcher dispatcher)
    {
        var layout = new DualLayout(height: this.config.Height);

        var search = new SearchWidget();
        var capture = new CaptureWidget();
        var status = new MongoStatusWidget(services);

        layout.Register(capture.Key, capture, LayoutZone.Left);
        layout.Register(status.Key, status, LayoutZone.Right);
        layout.Register(search.Key, search, LayoutZone.Right);

        this.Init(layout);

        Loaded += (_, __) =>
        {
            search.SearchRequested += async (_, query) =>
            {
                var results = await controller.SearchAsync(query);
                // replace MessageBox with SearchPalette widget later
                MessageBox.Show($"Search executed for: {query}\nFound: {results.Count()} items");
            };

            capture.CaptureRequested += async (_, __) =>
            {
                // Create the draft resource - Possible TODO for the "Abandon draft" scenario.
                var id = await controller.CreateDraftNoteAsync();

                new ResourceEditorWindow(controller, id, "").Show();
            };

            search.SearchRequested += (_, text) =>
            {
                var pal = new SearchPaletteWindow(controller);
                pal.OpenResourceRequested += (_, rid) =>
                {
                    // open editor for that resource
                    _ = OpenEditorAsync(rid);
                };
                pal.Show();
                pal.Activate();
                // optionally prefill and run
                // pal.SetQuery(text);
            };


        };
    }

    public override void Exit() => currentApp.Shutdown();


    private async Task OpenEditorAsync(long rid)
    {
        var doc = await services.Resources.GetAsync(rid);
        var body = doc?.Artifact?.Body ?? "";
        new ResourceEditorWindow(controller, rid, body).Show();
    }

}
