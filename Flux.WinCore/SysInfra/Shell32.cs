namespace Flux.WinCore.SysInfra;

using System.Runtime.InteropServices;

public class Shell32
{
    [DllImport("shell32.dll", SetLastError = true)]
    public static extern uint SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);
}
