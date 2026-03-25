namespace Flux;

using Flux.WinCore.SysInfra;
using System.Windows;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        var app = new Application
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown
        };

        var win = new FluxBar(new DefaultConfig(), Application.Current);
        win.Initialize(Application.Current.Dispatcher);
        app.Run(win);
    }
}
