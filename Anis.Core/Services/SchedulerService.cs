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

    private Timer? _timer;
    private AppSettings _settings = new();
    private List<Clip> _clips = [];
    private readonly List<string> _recentlyPlayedIds = [];
    private readonly Random _random = new();
    private readonly string _storagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Anis");


    public SchedulerService(IClipRepository clipRepository, ISettingsStore settingsStore, IAudioPlayer audioPlayer)
    {
        _clipRepository = clipRepository;
        _settingsStore = settingsStore;
        _audioPlayer = audioPlayer;
    }

    public async void Start()
    {
        _settings = await _settingsStore.LoadAsync();
        _clips = (await _clipRepository.GetClipsAsync()).ToList();

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
        var eligibleClips = _clips
            .Where(c => c.IsEnabled && !_recentlyPlayedIds.Contains(c.Id))
            .ToList();

        if (eligibleClips.Count == 0)
        {
            Debug.WriteLine("No eligible clips to play. Either all are disabled or have been recently played.");
            // Optional: Clear the recent list if we run out of clips
            if (_clips.Any(c => c.IsEnabled)) _recentlyPlayedIds.Clear();
            return;
        }

        var randomIndex = _random.Next(0, eligibleClips.Count);
        var selectedClip = eligibleClips[randomIndex];

        Debug.WriteLine($"Selected clip: {selectedClip.Title}");

        // Add to recently played list and manage its size
        _recentlyPlayedIds.Add(selectedClip.Id);
        while (_recentlyPlayedIds.Count > _settings.AvoidRecentRepetitionsCount)
        {
            _recentlyPlayedIds.RemoveAt(0);
        }

        // Construct the full path for the audio file
        var fullPath = Path.Combine(_storagePath, selectedClip.FilePath);

        await _audioPlayer.PlayAsync(fullPath, _settings.Volume);
    }

    public void Stop()
    {
        _timer?.Change(Timeout.Infinite, 0); // Stops the timer
        _timer?.Dispose();
        _timer = null;
        Debug.WriteLine("Scheduler stopped.");
    }
}