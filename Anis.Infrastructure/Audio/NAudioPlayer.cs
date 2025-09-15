using Anis.Core.Interfaces;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Anis.Infrastructure.Audio;

public class NAudioPlayer : IAudioPlayer, IDisposable
{
    private WaveOutEvent? _outputDevice;
    private WaveStream? _waveStream;
    private TaskCompletionSource<bool>? _playbackCompletionSource;

    public Task PlayAsync(string filePath, double volume)
    {
        if (!File.Exists(filePath))
        {
            // Or log this error
            return Task.CompletedTask;
        }

        Stop(); // Stop any currently playing audio

        _playbackCompletionSource = new TaskCompletionSource<bool>();

        try
        {
            _outputDevice = new WaveOutEvent();
            _waveStream = CreateWaveStream(filePath);
            _outputDevice.Init(_waveStream);

            _outputDevice.Volume = (float)Math.Clamp(volume, 0.0, 1.0);

            _outputDevice.PlaybackStopped += OnPlaybackStopped;
            _outputDevice.Play();
        }
        catch (Exception) // Catch exceptions from invalid audio files
        {
            Stop();
            _playbackCompletionSource.TrySetResult(false);
        }

        return _playbackCompletionSource.Task;
    }

    public void Stop()
    {
        _outputDevice?.Stop();
        // The actual disposal happens in OnPlaybackStopped to avoid race conditions
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        Dispose();
        _playbackCompletionSource?.TrySetResult(true);
    }

    private static WaveStream CreateWaveStream(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".mp3" => new Mp3FileReader(filePath),
            ".wav" => new WaveFileReader(filePath),
            _ => throw new NotSupportedException("Unsupported audio format")
        };
    }

    public void Dispose()
    {
        _waveStream?.Dispose();
        _waveStream = null;
        _outputDevice?.Dispose();
        _outputDevice = null;
    }
}