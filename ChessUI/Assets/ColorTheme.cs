using System.Windows.Media;

namespace ChessUI;

public enum ColorTheme
{
    Default,
    Blue,
    Green,
    Purple,
    Dark
}

public class ColorThemeInfo
{
    public ColorTheme Theme { get; set; }
    public string Name { get; set; }
    public Color StrokeColor { get; set; }
    public Color FillColor { get; set; }
    public Color TextColor { get; set; }
    public Color ButtonColor { get; set; }

    public SolidColorBrush PreviewBrush => new SolidColorBrush(StrokeColor);
}
