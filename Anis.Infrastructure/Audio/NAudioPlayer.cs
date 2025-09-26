using Anis.Core.Interfaces;
using NAudio.Wave; // Only the base NAudio library is needed
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Anis.Infrastructure.Audio;

public class NAudioPlayer : IAudioPlayer, IDisposable
{
    private WaveOutEvent? _outputDevice;
    private WaveStream? _waveStream;
    private readonly object _lockObject = new();

    public Task PlayAsync(string filePath, double volume)
    {
        var tcs = new TaskCompletionSource<bool>();

        Task.Run(() =>
        {
            lock (_lockObject)
            {
                if (!File.Exists(filePath))
                {
                    Debug.WriteLine($"[ERROR] Audio file not found: {filePath}");
                    tcs.TrySetResult(false);
                    return;
                }

                DisposePlaybackResources();

                try
                {
                    _outputDevice = new WaveOutEvent();
                    _waveStream = CreateWaveStream(filePath); // Using the new robust method
                    _outputDevice.Init(_waveStream);
                    _outputDevice.Volume = (float)Math.Clamp(volume, 0.0, 1.0);

                    _outputDevice.PlaybackStopped += (sender, args) =>
                    {
                        DisposePlaybackResources();
                        tcs.TrySetResult(true);
                    };

                    _outputDevice.Play();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[FATAL] Failed to play audio file '{filePath}'. Original error: {ex.GetType().Name} - {ex.Message}");
                    DisposePlaybackResources();
                    tcs.TrySetResult(false);
                }
            }
        });

        return tcs.Task;
    }

    private void DisposePlaybackResources()
    {
        _waveStream?.Dispose();
        _waveStream = null;
        _outputDevice?.Dispose();
        _outputDevice = null;
    }

    private static WaveStream CreateWaveStream(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        // On modern Windows (7 and newer), MediaFoundationReader is the most robust and recommended reader for MP3.
        if (extension == ".mp3")
        {
            return new MediaFoundationReader(filePath);
        }
        else if (extension == ".wav")
        {
            return new WaveFileReader(filePath);
        }
        else
        {
            throw new NotSupportedException("Unsupported audio format");
        }
    }

    public void Stop()
    {
        lock (_lockObject)
        {
            DisposePlaybackResources();
        }
    }

    public void Dispose()
    {
        Stop();
    }
}