using Anis.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anis.Core.Interfaces;

public interface IClipRepository
{
    Task<IEnumerable<Reciter>> GetRecitersAsync();
    Task<IEnumerable<Clip>> GetClipsAsync();
    Task SaveClipsAsync(IEnumerable<Clip> clips);
}