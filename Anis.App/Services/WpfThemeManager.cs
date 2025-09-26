using Anis.Infrastructure.Storage;
using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Linq;

// We use aliases to solve the name collision between our domain and the library's
using AnisTheme = Anis.Core.Domain.Theme;
using IAnisThemeManager = Anis.Core.Interfaces.IThemeManager;

namespace Anis.App.Services;

public class WpfThemeManager : IAnisThemeManager
{
    private readonly JsonStorageProvider _storage;

    public WpfThemeManager(JsonStorageProvider storage)
    {
        _storage = storage;
    }

    public void ApplyTheme(AnisTheme theme)
    {
        // Get the PaletteHelper
        var paletteHelper = new PaletteHelper();

        // Get the current theme instance from the PaletteHelper
        var currentTheme = paletteHelper.GetTheme();

        // Set the base theme (Dark/Light)
        currentTheme.SetBaseTheme(theme.BaseTheme.ToLower() == "dark" ? BaseTheme.Dark : BaseTheme.Light);

        // Set the primary color
        Color primaryColor = (Color)ColorConverter.ConvertFromString(theme.PrimaryColor);
        currentTheme.SetPrimaryColor(primaryColor);

        // Apply the modified theme back to the application
        paletteHelper.SetTheme(currentTheme);
    }

    public async Task<List<AnisTheme>> GetThemesAsync()
    {
        // This is a small correction to convert IEnumerable to List
        var themes = await _storage.GetThemesAsync();
        return themes.ToList();
    }
}