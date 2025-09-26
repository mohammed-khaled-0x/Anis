using System.Threading.Tasks;

namespace Anis.Core.Interfaces;

public interface IAudioPlayer
{
    Task PlayAsync(string filePath, double volume);
    void Stop();
}