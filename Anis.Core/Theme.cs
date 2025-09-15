namespace Anis.Core.Domain;

public class Theme
{
    public string Name { get; set; } = string.Empty;
    public bool IsDarkMode { get; set; }
    public PopupTheme Popup { get; set; } = new();
}

public class PopupTheme
{
    public string Background { get; set; } = "#2B2B2B";
    public string Foreground { get; set; } = "#A9B7C6";
    public string BorderBrush { get; set; } = "#555555";
    public double Opacity { get; set; } = 0.95;
    public double CornerRadius { get; set; } = 8;
    public string FontFamily { get; set; } = "Segoe UI";
    public int TitleFontSize { get; set; } = 14;
    public int TextFontSize { get; set; } = 18;
}