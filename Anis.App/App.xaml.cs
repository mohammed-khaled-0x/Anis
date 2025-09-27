using Anis.App.MVVM.ViewModels;
using Anis.App.Services;
using Anis.Core.Interfaces;
using Anis.Core.Services;
using Anis.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using IAnisThemeManager = Anis.Core.Interfaces.IThemeManager; // Use an alias here too

namespace Anis.App;

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }
    private const string AppMutexName = "AnisApp-SingleInstanceMutex-E7A4A0B8";
    private static Mutex? _mutex;
    private readonly string _storagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Anis");

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                // Register the single data provider instance
                services.AddSingleton<JsonStorageProvider>();

                // Register interfaces to point to the specific provider
                services.AddSingleton<ISettingsStore>(sp => sp.GetRequiredService<JsonStorageProvider>());
                services.AddSingleton<IClipRepository>(sp => sp.GetRequiredService<JsonStorageProvider>());

                // Register other infrastructure
                services.AddSingleton<IAudioPlayer, Anis.Infrastructure.Audio.NAudioPlayer>();
                services.AddSingleton<IAnisThemeManager, WpfThemeManager>();
                services.AddSingleton<INotificationHost, WpfNotificationHost>();
                services.AddTransient<ClipsManagerViewModel>();
                // Register Core Services
                services.AddSingleton<IScheduler, SchedulerService>();

                // Register ViewModels
                services.AddSingleton<SettingsViewModel>();

                services.AddTransient<AddClipViewModel>();


                // Register Views
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        _mutex = new Mutex(true, AppMutexName, out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("تطبيق أنيس يعمل بالفعل في الخلفية.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            Application.Current.Shutdown();
            return;
        }

        Directory.CreateDirectory(_storagePath);
        await AppHost!.StartAsync();
        EnsureDataFilesExist();

        // Initialize and apply theme
        var settingsStore = AppHost.Services.GetRequiredService<ISettingsStore>();
        var themeManager = AppHost.Services.GetRequiredService<IAnisThemeManager>();
        var settings = await settingsStore.LoadAsync();
        var themes = await themeManager.GetThemesAsync();
        var activeTheme = themes.FirstOrDefault(t => t.Name == settings.ActiveThemeName) ?? themes.FirstOrDefault();
        if (activeTheme != null)
        {
            themeManager.ApplyTheme(activeTheme);
        }

        await settingsStore.SaveAsync(settings);

        var scheduler = AppHost.Services.GetRequiredService<IScheduler>();
        scheduler.Start();

        var startupForm = AppHost.Services.GetRequiredService<MainWindow>();
        startupForm.Show();
        startupForm.Hide();

        base.OnStartup(e);
    }

    // EnsureDataFilesExist method remains the same
    private void EnsureDataFilesExist()
    {
        Directory.CreateDirectory(_storagePath);
        string[] dataFiles = ["reciters.json", "clips.json", "themes.json"];
        foreach (var fileName in dataFiles)
        {
            var destPath = Path.Combine(_storagePath, fileName);
            if (!File.Exists(destPath))
            {
                var sourcePath = Path.Combine(AppContext.BaseDirectory, fileName);
                if (File.Exists(sourcePath)) File.Copy(sourcePath, destPath);
            }
        }
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