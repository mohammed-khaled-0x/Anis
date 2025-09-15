using Anis.Core.Domain;
using System.Threading.Tasks;

namespace Anis.Core.Interfaces;

public interface ISettingsStore
{
    Task<AppSettings> LoadAsync();
    Task SaveAsync(AppSettings settings);
}