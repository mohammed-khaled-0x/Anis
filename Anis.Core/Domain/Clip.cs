namespace Anis.Core.Domain;

public class Clip
{
    public string Id { get; set; } = string.Empty;
    public int ReciterId { get; set; }
    public string? FilePath { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public bool IsEnabled { get; set; }
}