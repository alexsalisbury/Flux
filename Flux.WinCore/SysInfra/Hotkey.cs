namespace Flux.WinCore.SysInfra;

using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

public sealed class Hotkey : IDisposable
{
    private readonly HwndSource source;
    private readonly int id;
    private readonly Action callback;

    public Hotkey(Window window, ModifierKeys mods, Key key, Action callback)
    {
        this.callback = callback;
        id = GetHashCode();
        var helper = new WindowInteropHelper(window);
        source = HwndSource.FromHwnd(helper.EnsureHandle());
        source.AddHook(HwndHook);
        User32.RegisterHotKey(helper.Handle, id, (uint)mods, (uint)KeyInterop.VirtualKeyFromKey(key));
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOTKEY = 0x0312;
        if (msg == WM_HOTKEY && wParam.ToInt32() == id)
        {
            callback();
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose() => User32.UnregisterHotKey(source.Handle, id);
}
