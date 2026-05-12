namespace Flux.Tests;

using Flux.WinCore.SysInfra;

public class Win32EnumTests
{
    // --- APPBARMESSAGE: values used by TopBar ---

    [Theory]
    [InlineData(APPBARMESSAGE.New, 0x00)]
    [InlineData(APPBARMESSAGE.Remove, 0x01)]
    [InlineData(APPBARMESSAGE.QueryPos, 0x02)]
    [InlineData(APPBARMESSAGE.SetPos, 0x03)]
    [InlineData(APPBARMESSAGE.Activate, 0x06)]
    public void AppBarMessage_MatchesWin32(APPBARMESSAGE msg, int expected)
    {
        Assert.Equal(expected, (int)msg);
    }

    // --- APPBARNOTIFY ---

    [Theory]
    [InlineData(APPBARNOTIFY.ABN_STATECHANGE, 0)]
    [InlineData(APPBARNOTIFY.ABN_POSCHANGED, 1)]
    [InlineData(APPBARNOTIFY.ABN_FULLSCREENAPP, 2)]
    [InlineData(APPBARNOTIFY.ABN_WINDOWARRANGE, 3)]
    public void AppBarNotify_MatchesWin32(APPBARNOTIFY notify, int expected)
    {
        Assert.Equal(expected, (int)notify);
    }

    // --- SETWINDOWPOS flags ---

    [Theory]
    [InlineData(SETWINDOWPOS.SWP_NOSIZE, 0x0001u)]
    [InlineData(SETWINDOWPOS.SWP_NOMOVE, 0x0002u)]
    [InlineData(SETWINDOWPOS.SWP_NOACTIVATE, 0x0010u)]
    public void SetWindowPos_MatchesWin32(SETWINDOWPOS flag, uint expected)
    {
        Assert.Equal(expected, (uint)flag);
    }

    [Fact]
    public void SetWindowPos_FlagsCombine()
    {
        var combined = SETWINDOWPOS.SWP_NOMOVE | SETWINDOWPOS.SWP_NOSIZE | SETWINDOWPOS.SWP_NOACTIVATE;
        Assert.True(combined.HasFlag(SETWINDOWPOS.SWP_NOMOVE));
        Assert.True(combined.HasFlag(SETWINDOWPOS.SWP_NOSIZE));
        Assert.True(combined.HasFlag(SETWINDOWPOS.SWP_NOACTIVATE));
        Assert.False(combined.HasFlag(SETWINDOWPOS.SWP_SHOWWINDOW));
    }

    [Fact]
    public void SetWindowPos_Aliases()
    {
        Assert.Equal(SETWINDOWPOS.SWP_FRAMECHANGED, SETWINDOWPOS.SWP_DRAWFRAME);
        Assert.Equal(SETWINDOWPOS.SWP_NOOWNERZORDER, SETWINDOWPOS.SWP_NOREPOSITION);
    }

    // --- SWPZORDER ---

    [Theory]
    [InlineData(SWPZORDER.HWND_BOTTOM, 1)]
    [InlineData(SWPZORDER.HWND_TOP, 0)]
    [InlineData(SWPZORDER.HWND_TOPMOST, -1)]
    [InlineData(SWPZORDER.HWND_NOTOPMOST, -2)]
    public void SwpZOrder_MatchesWin32(SWPZORDER z, int expected)
    {
        Assert.Equal(expected, (int)z);
    }

    // --- WINDOWMESSAGE: values actually used in code ---

    [Fact]
    public void WM_ACTIVATE_Is0x0006()
    {
        Assert.Equal(0x0006u, (uint)WINDOWMESSAGE.WM_ACTIVATE);
    }

    [Fact]
    public void WM_HOTKEY_Is0x0312()
    {
        Assert.Equal(0x0312u, (uint)WINDOWMESSAGE.WM_HOTKEY);
    }

    // --- WINDOWSTYLE: values used in TopBar ---

    [Fact]
    public void WS_EX_TOOLWINDOW_Is0x80()
    {
        Assert.Equal(0x00000080u, (uint)WINDOWSTYLE.WS_EX_TOOLWINDOW);
    }

    // --- GETWINDOWLONG ---

    [Theory]
    [InlineData(GETWINDOWLONG.GWL_STYLE, -16)]
    [InlineData(GETWINDOWLONG.GWL_EXSTYLE, -20)]
    public void GetWindowLong_MatchesWin32(GETWINDOWLONG gwl, int expected)
    {
        Assert.Equal(expected, (int)gwl);
    }

    // --- MONITOR_DPI_TYPE ---

    [Theory]
    [InlineData(MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, 0)]
    [InlineData(MONITOR_DPI_TYPE.MDT_ANGULAR_DPI, 1)]
    [InlineData(MONITOR_DPI_TYPE.MDT_RAW_DPI, 2)]
    public void MonitorDpiType_MatchesWin32(MONITOR_DPI_TYPE dpi, int expected)
    {
        Assert.Equal(expected, (int)dpi);
    }

    // --- PROCESS_DPI_AWARENESS ---

    [Theory]
    [InlineData(PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE, 0)]
    [InlineData(PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE, 1)]
    [InlineData(PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE, 2)]
    public void ProcessDpiAwareness_MatchesWin32(PROCESS_DPI_AWARENESS dpi, int expected)
    {
        Assert.Equal(expected, (int)dpi);
    }
}
