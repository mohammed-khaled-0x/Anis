using Anis.Core.Domain;

namespace Anis.Core.Interfaces;

public interface INotificationHost
{
    void ShowNotification(Clip clip, Reciter reciter);
    void HideNotification();
}