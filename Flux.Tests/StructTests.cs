namespace Flux.Tests;

using Flux.WinCore.SysInfra;
using System.Runtime.InteropServices;

public class StructTests
{
    // --- RECT ---

    [Fact]
    public void Rect_DefaultValues()
    {
        var r = new RECT();
        Assert.Equal(0, r.Left);
        Assert.Equal(0, r.Top);
        Assert.Equal(0, r.Right);
        Assert.Equal(0, r.Bottom);
    }

    [Fact]
    public void Rect_FieldsAssignable()
    {
        var r = new RECT { Left = 10, Top = 20, Right = 100, Bottom = 200 };
        Assert.Equal(10, r.Left);
        Assert.Equal(20, r.Top);
        Assert.Equal(100, r.Right);
        Assert.Equal(200, r.Bottom);
    }

    // --- POINT ---

    [Fact]
    public void Point_DefaultValues()
    {
        var p = new POINT();
        Assert.Equal(0, p.X);
        Assert.Equal(0, p.Y);
    }

    [Fact]
    public void Point_FieldsAssignable()
    {
        var p = new POINT { X = 42, Y = 99 };
        Assert.Equal(42, p.X);
        Assert.Equal(99, p.Y);
    }

    // --- APPBARDATA ---

    [Fact]
    public void AppBarData_DefaultValues()
    {
        var abd = new APPBARDATA();
        Assert.Equal(0u, abd.cbSize);
        Assert.Equal(0, (int)abd.hWnd);
        Assert.Equal(0u, abd.uEdge);
    }

    [Fact]
    public void AppBarData_SizeMatchesMarshal()
    {
        var abd = new APPBARDATA();
        abd.cbSize = (uint)Marshal.SizeOf<APPBARDATA>();
        Assert.True(abd.cbSize > 0);
    }

    [Fact]
    public void AppBarData_RectEmbedded()
    {
        var abd = new APPBARDATA
        {
            rc = new RECT { Left = 0, Top = 0, Right = 1920, Bottom = 40 }
        };
        Assert.Equal(1920, abd.rc.Right);
        Assert.Equal(40, abd.rc.Bottom);
    }
}
