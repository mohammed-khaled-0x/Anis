using System.ComponentModel;
using System.Windows;

namespace Anis.App;

public partial class MainWindow : Window
{
    private bool _isExplicitlyClosed = false;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        // If the user is closing the window via the 'Exit' menu item, allow it.
        if (_isExplicitlyClosed)
        {
            return;
        }

        // Otherwise, cancel the close operation and just hide the window.
        e.Cancel = true;
        this.Hide();
    }

    private void MyNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
    {
        // Show the window and bring it to the front
        this.Show();
        this.WindowState = WindowState.Normal;
        this.Activate();
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        // This is the same as double-clicking
        MyNotifyIcon_TrayMouseDoubleClick(sender, e);
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        _isExplicitlyClosed = true; // Mark that the exit was intentional
        Application.Current.Shutdown();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            this.Hide();
        }
        base.OnStateChanged(e);
    }
}