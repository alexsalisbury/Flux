namespace Flux.WinCore.SysInfra;

public delegate nint WndProc(nint hWnd, int uMsg, nint wParam, nint lParam, ref bool handled);
