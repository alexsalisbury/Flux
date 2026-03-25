namespace Flux;

using Flux.WinCore;
using Flux.WinCore.SysInfra;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

public sealed class FluxBar : TopBar
{
    private Application currentApp;
    private Hotkey? hotkey;

    public FluxBar(IConfig cfg, Application current) : base(cfg)
    {
        currentApp = current;
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
        this.Init(new DualLayout());
    }

    public override void Exit() => currentApp.Shutdown();
}
