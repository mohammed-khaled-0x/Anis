using Anis.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anis.Core.Interfaces;

public interface IThemeManager
{
    Task<List<Theme>> GetThemesAsync();
    void ApplyTheme(Theme theme);
}