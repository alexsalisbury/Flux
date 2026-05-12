namespace Flux.WinCore;

using Flux.WinCore.SysInfra;
using Flux.WinCore.Widgets;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

public abstract class TopBar : Window
{
    IWidgetManager wm;
    bool isPostLoad;
    protected IConfig config;
    protected APPBARDATA abd = new();
    protected nint hWnd;
    public static double Scale = 1;
    protected bool barTransparent = false;
    // these are scaled, to get original multiply with scale
    public static int screenWidth;
    public static int screenHeight;

    protected ILayout Layout { get; private set; }
    public abstract void Exit();


    protected TopBar(IConfig cfg)
    {
        config = cfg;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Topmost = true;


        Unloaded += (_, __) =>
        {
            wm?.StopAll();
        };
    }

    /// <summary>
    /// Allows us to claim desktop real estate
    /// Does not work in SourceInitialized, needs at lest Loaded
    /// </summary>
    public void RegisterAsAppbar()
    {
        abd.cbSize = (uint)Marshal.SizeOf<APPBARDATA>();
        abd.hWnd = this.hWnd;
        abd.uCallbackMessage = User32.RegisterWindowMessage("TopbarMessage");

        uint res = Shell32.SHAppBarMessage((uint)APPBARMESSAGE.New, ref abd);

        AppbarSetPos();
    }

    public void UnregisterAsAppbar()
    {
        uint res = Shell32.SHAppBarMessage((uint)APPBARMESSAGE.Remove, ref abd);
    }

    public void AppbarSetPos()
    {
        var edge = AppBarLayout.ParseEdge(config.DockEdge);
        abd.uEdge = (uint)edge;

        var initial = AppBarLayout.ComputeInitialRect(edge, screenWidth, screenHeight, Scale);
        abd.rc = new() { Left = initial.Left, Top = initial.Top, Right = initial.Right, Bottom = initial.Bottom };

        Shell32.SHAppBarMessage((uint)APPBARMESSAGE.QueryPos, ref abd);

        var adjusted = AppBarLayout.ComputeAdjustedRect(
            edge, initial, screenHeight, Scale,
            config.Height, config.MarginYTop, config.MarginYBottom);
        abd.rc = new() { Left = adjusted.Left, Top = adjusted.Top, Right = adjusted.Right, Bottom = adjusted.Bottom };

        Shell32.SHAppBarMessage((uint)APPBARMESSAGE.SetPos, ref abd);
    }


    private void WindowInit()
    {
        Shcore.SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);

        screenWidth = User32.GetSystemMetrics(0);
        screenHeight = User32.GetSystemMetrics(1);

        TopBar.Scale = GetDisplayScaling();
        //		Logger.Log($"Scale factor: {scale}");
        screenWidth = (int)(screenWidth / Scale);
        screenHeight = (int)(screenHeight / Scale);

        if (config.Width == 0) { config.Width = screenWidth - (config.MarginXLeft + config.MarginXRight); }

        this.Background = ThemePalette.BrushFromHex(config.BackgroundColor);
        if (this.Background.Equals(Colors.Transparent)) { barTransparent = true; }

        //		// Make bar a toolwindow (appear always on top)
        //		// TODO: loses topmost to other windows when task manager is open
        uint exStyles = User32.GetWindowLong(hWnd, GETWINDOWLONG.GWL_EXSTYLE);
        User32.SetWindowLong(hWnd, (int)GETWINDOWLONG.GWL_EXSTYLE, (int)(exStyles | (uint)WINDOWSTYLE.WS_EX_TOOLWINDOW));

        //		Utils.HideWindowInAltTab(hWnd);

        this.Width = config.Width;
        this.Height = config.Height;
        this.Left = config.MarginXLeft;
        this.Top = AppBarLayout.ComputeWindowTop(
            config.DockEdge, screenHeight, config.Height,
            config.MarginYTop, config.MarginYBottom);


        this.BorderBrush = ThemePalette.BrushFromHex(config.BorderColor);
        this.BorderThickness = config.BorderThickness;


    }


    private static double GetDisplayScaling()
    {
        nint hMon = User32.MonitorFromPoint(new POINT() { X = 0, Y = 0 }, 0x01);
        Shcore.GetDpiForMonitor(hMon, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);
        return dpiX / 96.0f;
    }

    protected nint WndProcLocal(nint hWnd, int msg, nint wparam, nint lparam, ref bool handled)
    {
        switch (msg)
        {
            case (int)WINDOWMESSAGE.WM_ACTIVATE:
                Shell32.SHAppBarMessage((int)APPBARMESSAGE.Activate, ref abd);
                break;
        }

        switch (wparam)
        {
            case (int)APPBARNOTIFY.ABN_POSCHANGED:
                // AppbarSetPos();
                break;

            case (int)APPBARNOTIFY.ABN_FULLSCREENAPP:
                if (lparam > 0) // fullscreen app is opening
                {
                    if (this.Topmost)
                    {
                        this.Topmost = false;
                        User32.SetWindowPos(this.hWnd, (nint)SWPZORDER.HWND_BOTTOM, 0, 0, 0, 0, SETWINDOWPOS.SWP_NOMOVE | SETWINDOWPOS.SWP_NOSIZE | SETWINDOWPOS.SWP_NOACTIVATE);
                    }
                }
                else // revert back to topmost once fullscreen app closes
                {
                    if (!this.Topmost)
                    {
                        User32.SetWindowPos(this.hWnd, (nint)SWPZORDER.HWND_TOPMOST, 0, 0, 0, 0, SETWINDOWPOS.SWP_NOMOVE | SETWINDOWPOS.SWP_NOSIZE | SETWINDOWPOS.SWP_NOACTIVATE);
                        this.Topmost = true;
                    }
                }

                break;
        }

        return 0;
    }

    public void Build(ILayout baseLayout)
    {
        this.Layout = baseLayout;

        this.Title = "Bar";
        this.WindowStyle = WindowStyle.None;
        this.AllowsTransparency = true;
        this.Topmost = true;

        // WPF event sequence
        // https://memories3615.wordpress.com/2017/03/24/wpf-window-events-sequence/
        SourceInitialized += (s, e) =>
        {
            hWnd = new WindowInteropHelper(this).Handle;
            WindowInit(); // needs hWnd
            this.Hook(WndProcLocal);
        };

        Loaded += (s, e) =>
        {
            if (isPostLoad) return;

            RegisterAsAppbar();

            this.Content = Layout.Container;

            this.wm = new WidgetManager(Layout);
            wm.StartAll();

            isPostLoad = true;
        };
    }

    protected void Init(ILayout layout)
    {
        Build(layout);

        this.ContextMenu = this.CreateThemedContextMenu([
            ("Exit", (s, e) => { Exit(); }),
                ]);
    }

    /// <summary>
    /// Right click context menu with menubuttons
    /// <summary>
    public ContextMenu CreateContextMenu(List<(string, Action<object, object>)> nameActionPairs)
    {
        var ctxMenu = new ContextMenu();

        foreach (var pair in nameActionPairs)
        {
            var menuItem = new MenuItem() { Header = pair.Item1 };
            menuItem.Click += (s, e) => pair.Item2(s, e);
            ctxMenu.Items.Add(menuItem);
        }

        return ctxMenu;
    }


    public ContextMenu CreateThemedContextMenu(List<(string Header, Action<object, object> Action)> items)
    {
        var ctx = new ContextMenu
        {
            // Base menu styling
            Background = ThemePalette.BrushNearBlack,
            BorderBrush = ThemePalette.BrushCharcoal,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(0),
            SnapsToDevicePixels = true
        };
        // ----- MenuItem Style -----
        var itemStyle = new Style(typeof(MenuItem))
        {
            // Default appearance
            Setters =
    {
        new Setter(MenuItem.PaddingProperty, new Thickness(12, 4, 10, 4)),
        new Setter(MenuItem.CursorProperty, Cursors.Arrow),
        new Setter(MenuItem.BackgroundProperty, ThemePalette.BrushNearBlack),
        new Setter(MenuItem.ForegroundProperty, ThemePalette.BrushGray600),
        new Setter(MenuItem.BorderThicknessProperty, new Thickness(0))
    }
        };

        // ----- Custom ControlTemplate (NO icon/checkmark gutter) -----
        var template = new ControlTemplate(typeof(MenuItem));

        FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
        border.Name = "border";

        border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(MenuItem.BackgroundProperty));
        border.SetValue(Border.SnapsToDevicePixelsProperty, true);

        FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
        content.SetValue(ContentPresenter.ContentSourceProperty, "Header");
        content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        content.SetValue(ContentPresenter.MarginProperty, new Thickness(0, 0, 0, 0));

        border.AppendChild(content);
        template.VisualTree = border;

        itemStyle.Setters.Add(new Setter(MenuItem.TemplateProperty, template));


        // ----- HOVER -----
        var hoverTrigger = new Trigger
        {
            Property = MenuItem.IsHighlightedProperty,
            Value = true
        };
        hoverTrigger.Setters.Add(new Setter(MenuItem.BackgroundProperty, ThemePalette.BrushCharcoal));
        hoverTrigger.Setters.Add(new Setter(MenuItem.ForegroundProperty, Brushes.White));
        itemStyle.Triggers.Add(hoverTrigger);


        // ----- PRESSED -----
        var pressedTrigger = new Trigger
        {
            Property = MenuItem.IsPressedProperty,
            Value = true
        };
        pressedTrigger.Setters.Add(new Setter(MenuItem.BackgroundProperty, ThemePalette.BrushFromHex("#3C3C3C")));
        pressedTrigger.Setters.Add(new Setter(MenuItem.ForegroundProperty, Brushes.White));
        itemStyle.Triggers.Add(pressedTrigger);


        // ----- DISABLED -----
        var disabledTrigger = new Trigger
        {
            Property = MenuItem.IsEnabledProperty,
            Value = false
        };
        disabledTrigger.Setters.Add(new Setter(MenuItem.ForegroundProperty, new SolidColorBrush(Color.FromRgb(90, 90, 90))));
        disabledTrigger.Setters.Add(new Setter(MenuItem.BackgroundProperty, ThemePalette.BrushNearBlack));
        itemStyle.Triggers.Add(disabledTrigger);


        // ----- Apply style into ContextMenu.Resources -----
        ctx.Resources[typeof(MenuItem)] = itemStyle;

        // Create the items
        foreach (var pair in items)
        {
            var mi = new MenuItem
            {
                Header = pair.Header,
                Padding = new Thickness(10, 4, 10, 4),
                Cursor = Cursors.Arrow
            };

            mi.Click += (s, e) => pair.Action(s, e);
            ctx.Items.Add(mi);
        }

        return ctx;
    }
}
