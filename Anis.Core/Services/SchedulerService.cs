using Anis.Core.Domain;
using Anis.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anis.Core.Services;

public class SchedulerService : IScheduler
{
    private readonly IClipRepository _clipRepository;
    private readonly ISettingsStore _settingsStore;
    private readonly IAudioPlayer _audioPlayer;
    private readonly INotificationHost _notificationHost;

    private Timer? _timer;
    private AppSettings _settings = new();
    private List<Clip> _clips = [];
    private List<Reciter> _reciters = [];
    private readonly List<string> _recentlyPlayedIds = [];
    private readonly Random _random = new();
    private bool _isPaused = false;
    public bool IsPaused => _isPaused;
    private readonly string _storagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Anis");


    public SchedulerService(IClipRepository clipRepository, ISettingsStore settingsStore, IAudioPlayer audioPlayer, INotificationHost notificationHost)
    {
        _clipRepository = clipRepository;
        _settingsStore = settingsStore;
        _audioPlayer = audioPlayer;
        _notificationHost = notificationHost;
    }

    public async void Start()
    {
        _settings = await _settingsStore.LoadAsync();
        _clips = (await _clipRepository.GetClipsAsync()).ToList();
        _reciters = (await _clipRepository.GetRecitersAsync()).ToList();
        // For testing, let's set a shorter interval
        // In production, the user's setting will be used.
        // _settings.IntervalMinutes = 1; // You can uncomment this for faster testing
        var interval = TimeSpan.FromMinutes(_settings.IntervalMinutes);

        // Start the timer. It will wait for the interval before the first tick.
        _timer = new Timer(OnTimerTick, null, interval, interval);

        Debug.WriteLine($"Scheduler started. Next notification in {interval.TotalMinutes} minutes.");
    }

    private async void OnTimerTick(object? state)
    {
        if (IsPaused)
        {
            Debug.WriteLine("Scheduler is paused. Skipping tick.");
            return;
        }

        var eligibleClips = _clips
            .Where(c => c.IsEnabled && !_recentlyPlayedIds.Contains(c.Id))
            .ToList();

        if (eligibleClips.Count == 0)
        {
            Debug.WriteLine("No eligible clips to play. Either all are disabled or have been recently played.");
            if (_clips.Any(c => c.IsEnabled)) _recentlyPlayedIds.Clear();
            return;
        }

        var selectedClip = eligibleClips[_random.Next(0, eligibleClips.Count)];
        var reciter = _reciters.FirstOrDefault(r => r.Id == selectedClip.ReciterId);

        if (reciter == null)
        {
            Debug.WriteLine($"Reciter with ID {selectedClip.ReciterId} not found for clip {selectedClip.Title}.");
            return;
        }

        _notificationHost.ShowNotification(selectedClip, reciter);

        Task completionTask;

        // Check if the clip has an audio file
        if (!string.IsNullOrWhiteSpace(selectedClip.FilePath))
        {
            // It's an audio clip, so we play the audio
            var fullPath = Path.Combine(_storagePath, selectedClip.FilePath);
            completionTask = _audioPlayer.PlayAsync(fullPath, _settings.Volume);
        }
        else
        {
            // It's a text-only clip. We create a manual delay task.
            // Let's use a fixed duration for now (e.g., 7 seconds). We can make this configurable later.
            completionTask = Task.Delay(TimeSpan.FromSeconds(7));
        }

        // Await EITHER the audio finishing OR the manual delay finishing.
        await completionTask;

        await Task.Delay(500); // Grace period
        _notificationHost.HideNotification();
        _recentlyPlayedIds.Add(selectedClip.Id);
        while (_recentlyPlayedIds.Count > _settings.AvoidRecentRepetitionsCount)
        {
            _recentlyPlayedIds.RemoveAt(0);
        }
    }

    public void Stop()
    {
        _timer?.Change(Timeout.Infinite, 0); // Stops the timer
        _timer?.Dispose();
        _timer = null;
        Debug.WriteLine("Scheduler stopped.");
    }

    public void Pause()
    {
        if (_isPaused) return;
        _isPaused = true;
        Debug.WriteLine("Scheduler paused.");
    }
    public void Resume()
    {
        if (!_isPaused) return;
        _isPaused = false;
        Debug.WriteLine("Scheduler resumed.");
    }

    public void UpdateSettings(AppSettings newSettings)
    {
        _settings = newSettings;

        // Stop the current timer
        _timer?.Change(Timeout.Infinite, 0);
        _timer?.Dispose();

        // Start a new timer with the updated interval
        var newInterval = TimeSpan.FromMinutes(_settings.IntervalMinutes);
        _timer = new Timer(OnTimerTick, null, newInterval, newInterval);

        Debug.WriteLine($"Scheduler settings updated. New interval: {newInterval.TotalMinutes} minutes.");
    }

    public async Task RefreshDataAsync()
    {
        _clips = (await _clipRepository.GetClipsAsync()).ToList();
        _reciters = (await _clipRepository.GetRecitersAsync()).ToList();
        _recentlyPlayedIds.Clear(); // Clear recent list to allow new clips to play
        Debug.WriteLine("Scheduler data refreshed.");
    }
}