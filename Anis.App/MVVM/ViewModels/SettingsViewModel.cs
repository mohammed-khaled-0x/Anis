using Anis.Core.Domain;
using Anis.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Anis.App.MVVM.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsStore _settingsStore;
    private readonly IScheduler _scheduler;
    private readonly IThemeManager _themeManager;
    private AppSettings _settings;

    [ObservableProperty] private int _intervalMinutes;
    [ObservableProperty] private bool _runOnWindowsStartup;
    [ObservableProperty] private List<Theme> _availableThemes = new();
    [ObservableProperty] private Theme? _selectedTheme;

    public SettingsViewModel(ISettingsStore settingsStore, IScheduler scheduler, IThemeManager themeManager)
    {
        _settingsStore = settingsStore;
        _scheduler = scheduler;
        _themeManager = themeManager;
        _settings = new AppSettings();
        PresetIntervals = new List<int> { 5, 10, 15, 30, 60, 120 };

        LoadData();
    }

    partial void OnSelectedThemeChanged(Theme? value)
    {
        if (value != null)
        {
            _themeManager.ApplyTheme(value);
            _settings.ActiveThemeName = value.Name;
        }
    }

    private async void LoadData()
    {
        _settings = await _settingsStore.LoadAsync();
        AvailableThemes = await _themeManager.GetThemesAsync();

        IntervalMinutes = _settings.IntervalMinutes;
        RunOnWindowsStartup = _settings.RunOnWindowsStartup;
        SelectedTheme = AvailableThemes.FirstOrDefault(t => t.Name == _settings.ActiveThemeName) ?? AvailableThemes.FirstOrDefault();
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        _settings.IntervalMinutes = IntervalMinutes;
        _settings.RunOnWindowsStartup = RunOnWindowsStartup;
        _settings.ActiveThemeName = SelectedTheme?.Name ?? "Anis Dark (Default)";

        await _settingsStore.SaveAsync(_settings);
        _scheduler.UpdateSettings(_settings);
    }

    public List<int> PresetIntervals { get; }
}