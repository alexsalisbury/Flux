namespace Flux.WinCore.SysInfra;

using System.Runtime.InteropServices;

public class User32
{
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint GetWindowLong(nint hWnd, GETWINDOWLONG nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public extern static nint MonitorFromPoint(POINT pt, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint RegisterWindowMessage(string message);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, SETWINDOWPOS uFlags);

    [DllImport("user32.dll")]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
