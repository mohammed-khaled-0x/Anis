using Anis.App.Views;
using Anis.Core.Domain;
using Anis.Core.Interfaces;
using System.Windows;

namespace Anis.App.Services;

public class WpfNotificationHost : INotificationHost
{
    private NotificationPopup? _currentPopup;

    public void ShowNotification(Clip clip, Reciter reciter)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Close any old popup instantly before showing a new one.
            _currentPopup?.Close();

            _currentPopup = new NotificationPopup();
            _currentPopup.SetData(clip, reciter);
            _currentPopup.Show();
        });
    }

    public void HideNotification()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (_currentPopup != null && _currentPopup.IsVisible)
            {
                _currentPopup.CloseWithAnimation();
                _currentPopup = null;
            }
        });
    }
}