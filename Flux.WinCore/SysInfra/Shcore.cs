namespace Flux.WinCore.SysInfra;

using System.Runtime.InteropServices;

public class Shcore
{
    [DllImport("shcore.dll", SetLastError = true)]
    public static extern int GetDpiForMonitor(nint hMon, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY);

    //	// call this with PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE
    //	// so that all functions like GetSystemMetric() and GetDpiForMonitor()
    //	// returns the actual/correct values
    [DllImport("shcore.dll", SetLastError = true)]
    public static extern int SetProcessDpiAwareness(PROCESS_DPI_AWARENESS value);
}
