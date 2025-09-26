using Anis.Core.Domain;

namespace Anis.Core.Interfaces;

public interface IScheduler
{
    void Start();
    void Stop();
    void UpdateSettings(AppSettings newSettings);
    void Pause();
    void Resume();
    bool IsPaused { get; }
    Task RefreshDataAsync();
}