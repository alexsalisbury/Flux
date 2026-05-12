namespace Flux.WinCore;

public enum AppBarEdge { Left = 0, Top = 1, Right = 2, Bottom = 3 }

public readonly record struct AppBarRect(int Left, int Top, int Right, int Bottom);

public static class AppBarLayout
{
    public static AppBarEdge ParseEdge(string dockEdge) => dockEdge switch
    {
        "left" => AppBarEdge.Left,
        "top" => AppBarEdge.Top,
        "right" => AppBarEdge.Right,
        "bottom" => AppBarEdge.Bottom,
        _ => AppBarEdge.Top,
    };

    public static AppBarRect ComputeInitialRect(AppBarEdge edge, int screenWidth, int screenHeight, double scale)
    {
        return edge switch
        {
            AppBarEdge.Left or AppBarEdge.Right => new AppBarRect(0, 0, 0, (int)(screenHeight * scale)),
            _ => new AppBarRect(0, 0, (int)(screenWidth * scale), 0),
        };
    }

    public static AppBarRect ComputeAdjustedRect(
        AppBarEdge edge,
        AppBarRect initial,
        int screenHeight,
        double scale,
        int barHeight,
        int marginYTop,
        int marginYBottom)
    {
        return edge switch
        {
            AppBarEdge.Top => initial with
            {
                Bottom = (int)((barHeight + 2 * marginYTop) * scale)
            },
            AppBarEdge.Bottom => initial with
            {
                Top = (int)((screenHeight - barHeight - 2 * marginYBottom) * scale),
                Bottom = (int)(screenHeight * scale)
            },
            _ => initial,
        };
    }

    public static int ComputeWindowTop(string dockEdge, int screenHeight, int barHeight, int marginYTop, int marginYBottom)
    {
        return dockEdge switch
        {
            "top" => marginYTop,
            "bottom" => screenHeight - (barHeight + marginYBottom),
            _ => marginYTop,
        };
    }
}
