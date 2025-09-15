namespace Anis.Core.Domain;

public class AppSettings
{
    public int Version { get; set; } = 1;
    public int IntervalMinutes { get; set; } = 15;
    public bool IsRandom { get; set; } = true;
    public int AvoidRecentRepetitionsCount { get; set; } = 10;
    public ActiveTimeWindow ActiveTimeWindow { get; set; } = new();
    public double Volume { get; set; } = 0.8;
    public bool RunOnWindowsStartup { get; set; } = false;
    public string ActiveThemeName { get; set; } = "Darkula";
    public bool Mute { get; set; } = false;
}

public class ActiveTimeWindow
{
    public bool IsEnabled { get; set; } = true;
    public string StartTime { get; set; } = "08:00";
    public string EndTime { get; set; } = "23:00";
}