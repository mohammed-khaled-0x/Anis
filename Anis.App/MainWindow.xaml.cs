using Anis.App.MVVM.ViewModels;
using Anis.Core.Interfaces;
using System.ComponentModel;
using System.Windows;
using System; // Required for OnStateChanged

namespace Anis.App;

public partial class MainWindow : Window
{
    private bool _isExplicitlyClosed = false;
    private readonly IScheduler _scheduler;

    public MainWindow(SettingsViewModel viewModel, IScheduler scheduler)
    {
        InitializeComponent();
        DataContext = viewModel;
        _scheduler = scheduler;
    }

    // --- CENTRALIZED FUNCTION TO SHOW THE WINDOW ---
    private void ShowSettingsWindow()
    {
        this.Show();
        this.WindowState = WindowState.Normal;
        this.Activate();
        this.Focus(); // Ensure it gets keyboard focus
    }

    // --- EVENT HANDLERS ---

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_isExplicitlyClosed) return;
        e.Cancel = true;
        this.Hide();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            this.Hide();
        }
        base.OnStateChanged(e);
    }

    // --- TRAY ICON INTERACTIONS ---

    private void MyNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
    {
        ShowSettingsWindow(); // Call the centralized function
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        ShowSettingsWindow(); // Call the centralized function
    }

    private void PauseMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_scheduler.IsPaused)
        {
            _scheduler.Resume();
            PauseMenuItem.IsChecked = false;
        }
        else
        {
            _scheduler.Pause();
            PauseMenuItem.IsChecked = true;
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        _isExplicitlyClosed = true;
        MyNotifyIcon.Dispose(); // Clean up the tray icon
        Application.Current.Shutdown();
    }
}