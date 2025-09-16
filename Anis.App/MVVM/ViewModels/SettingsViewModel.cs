using Anis.Core.Domain;
using Anis.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Anis.App.MVVM.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsStore _settingsStore;
    private readonly IScheduler _scheduler;
    private AppSettings _settings;

    [ObservableProperty]
    private int _intervalMinutes;

    [ObservableProperty]
    private bool _runOnWindowsStartup;

    public List<int> PresetIntervals { get; private set; }
    public SettingsViewModel(ISettingsStore settingsStore, IScheduler scheduler)
    {
        _settingsStore = settingsStore;
        _scheduler = scheduler;
        _settings = new AppSettings(); // Initialize with default
        PresetIntervals = new List<int> { 5, 10, 15, 30, 60, 120 };
        LoadSettings();
    }

    private async void LoadSettings()
    {
        _settings = await _settingsStore.LoadAsync();
        IntervalMinutes = _settings.IntervalMinutes;
        RunOnWindowsStartup = _settings.RunOnWindowsStartup;
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        // Update the settings object from the properties
        _settings.IntervalMinutes = IntervalMinutes;
        _settings.RunOnWindowsStartup = _runOnWindowsStartup;

        // Save to the JSON file
        await _settingsStore.SaveAsync(_settings);

        // Notify the scheduler about the changes in real-time
        _scheduler.UpdateSettings(_settings);
    }
}