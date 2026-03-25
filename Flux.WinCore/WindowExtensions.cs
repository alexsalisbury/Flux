namespace Flux.WinCore;

using Flux.WinCore.SysInfra;
using System.Windows;
using System.Windows.Interop;

public static class WindowExtensions
{
    public static void Hook(this Window wnd, WndProc proc)
    {
        HwndSource hWndSrc = HwndSource.FromHwnd(new WindowInteropHelper(wnd).EnsureHandle());
        HwndSourceHook hook = new(proc);
        hWndSrc.AddHook(hook);
        wnd.Closing += (s, e) =>
        {
            hWndSrc.RemoveHook(hook);
        };
    }
}
