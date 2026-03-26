namespace Flux.WinCore;

using System.Windows;

public interface IConfig
{
    int Width { get; set; }
    int Height { get; }
    int MarginXLeft { get; }
    int MarginXRight { get; }
    int MarginYTop { get; }
    int MarginYBottom { get; }
    string DockEdge { get; }
    string BackgroundColor { get; }
    string BorderColor { get; }
    Thickness BorderThickness { get; }
}

public class DefaultConfig : IConfig
{
    public int Height { get; } = 40;
    public int Width { get; set; } = 0;
    public string DockEdge { get; } = "top";
    public int MarginXLeft { get; } = 0;
    public int MarginXRight { get; } = 0;
    public int MarginYTop { get; } = 0;
    public int MarginYBottom { get; } = 0;
    public int PaddingXLeft { get; } = 0;
    public int PaddingXRight { get; } = 0;
    public int PaddingYTop { get; } = 0;
    public int PaddingYDown { get; } = 0;
    public string BackgroundColor { get; } = "#111111";
    public bool RoundedCorners { get; } = true;
    public string BorderColor { get; } = "#00FF00";
    public Thickness BorderThickness { get; } = new(0);
    public bool HardwareRendering { get; } = true;
}
