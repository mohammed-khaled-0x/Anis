using Anis.Core.Interfaces;
using Anis.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Windows;
using Anis.Infrastructure.Audio;
using Anis.Core.Services;
using System.Diagnostics;
using Anis.App.Services;

namespace Anis.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IHost? AppHost { get; private set; }
    private readonly string _storagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Anis");

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                // Register Services (Implementations)
                services.AddSingleton<ISettingsStore, JsonStorageProvider>();
                services.AddSingleton<IClipRepository, JsonStorageProvider>();
                services.AddSingleton<IAudioPlayer, Anis.Infrastructure.Audio.NAudioPlayer>();
                services.AddSingleton<IScheduler, SchedulerService>();
                services.AddSingleton<INotificationHost, WpfNotificationHost>();
                // Register Views (Windows)
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        Directory.CreateDirectory(_storagePath);

        await AppHost!.StartAsync();

        EnsureDataFilesExist();

        var settingsStore = AppHost.Services.GetRequiredService<ISettingsStore>();
        var currentSettings = await settingsStore.LoadAsync();
        await settingsStore.SaveAsync(currentSettings);

        // Start the scheduler in the background
        var scheduler = AppHost.Services.GetRequiredService<IScheduler>();
        scheduler.Start();

        // The main window is now optional, we can still show it for settings later.
        var startupForm = AppHost.Services.GetRequiredService<MainWindow>();
        startupForm.Show();

        base.OnStartup(e);
    }

    // في ملف App.xaml.cs
    private void EnsureDataFilesExist()
    {
        Directory.CreateDirectory(_storagePath);

        // Copy data files (JSON)
        string[] dataFiles = ["reciters.json", "clips.json"];
        foreach (var fileName in dataFiles)
        {
            var destPath = Path.Combine(_storagePath, fileName);
            if (!File.Exists(destPath))
            {
                var sourcePath = Path.Combine(AppContext.BaseDirectory, fileName);
                if (File.Exists(sourcePath)) File.Copy(sourcePath, destPath);
            }
        }

        // Copy clip files (MP3, WAV, etc.)
        var sourceClipsDir = Path.Combine(AppContext.BaseDirectory, "clips");
        var destClipsDir = Path.Combine(_storagePath, "clips");
        Directory.CreateDirectory(destClipsDir);

        if (Directory.Exists(sourceClipsDir))
        {
            foreach (var sourceClipPath in Directory.GetFiles(sourceClipsDir))
            {
                var clipFileName = Path.GetFileName(sourceClipPath);
                var destClipPath = Path.Combine(destClipsDir, clipFileName);
                if (!File.Exists(destClipPath))
                {
                    File.Copy(sourceClipPath, destClipPath);
                }
            }
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost!.StopAsync();
        base.OnExit(e);
    }
}