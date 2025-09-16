using Anis.Core.Domain;
using Anis.Core.Interfaces;
using Anis.App.Views;
using System.Windows;
using System.Windows.Threading;
using System;

namespace Anis.App.Services;

public class WpfNotificationHost : INotificationHost
{
    private NotificationPopup? _currentPopup;

    public void ShowNotification(Clip clip, Reciter reciter)
    {
        // WPF UI elements must be manipulated from the UI thread.
        // The Scheduler's timer runs on a background thread, so we must dispatch.
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Close any existing popup before showing a new one
            _currentPopup?.Close();

            _currentPopup = new NotificationPopup();

            // Pass the data to the popup
            // Note: We will replace this with proper MVVM DataBinding later.
            // For now, this is a direct and simple way.
            _currentPopup.SetData(clip, reciter);

            _currentPopup.Show();

            // Auto-close the popup after a certain duration (e.g., 10 seconds)
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                _currentPopup?.CloseWithAnimation();
            };
            timer.Start();
        });
    }
}