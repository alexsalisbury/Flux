namespace Flux;

using Flux.FluxWindows;
using Flux.WinCore;
using Microsoft.Extensions.Configuration;
using System.Configuration;
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

        var environment =
                Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();

        var win = new FluxBar(configuration, new DefaultConfig(), Application.Current);
        win.Initialize(Application.Current.Dispatcher);
        app.Run(win);
    }
}
