using Anis.App.MVVM.ViewModels;
using Anis.App.Views;
using Anis.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Anis.App;

public partial class MainWindow : Window
{
    private bool _isExplicitlyClosed = false;
    private readonly IScheduler _scheduler;
    private readonly IServiceProvider _serviceProvider;

    public SettingsViewModel SettingsVM { get; }
    public ClipsManagerViewModel ClipsManagerVM { get; }

    public MainWindow(
        SettingsViewModel settingsVM,
        ClipsManagerViewModel clipsManagerVM,
        IScheduler scheduler,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();

        SettingsVM = settingsVM;
        ClipsManagerVM = clipsManagerVM;
        _scheduler = scheduler;
        _serviceProvider = serviceProvider;

        DataContext = this;
    }

    // --- Custom Title Bar Handlers ---
    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            this.DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Hide();
    }


    // --- Window Behavior Handlers ---
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

    // --- Tray Icon Handlers ---
    private void ShowSettingsWindow()
    {
        this.Show();
        this.WindowState = WindowState.Normal;
        this.Activate();
        this.Focus();
    }

    private void MyNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
    {
        ShowSettingsWindow();
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        ShowSettingsWindow();
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
        MyNotifyIcon.Dispose();
        Application.Current.Shutdown();
    }
}