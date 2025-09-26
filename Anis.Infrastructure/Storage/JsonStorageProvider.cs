using Anis.Core.Domain;
using Anis.Core.Interfaces;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Anis.Infrastructure.Storage;

// We removed IThemeManager from here. This class only reads/writes files.
public class JsonStorageProvider : ISettingsStore, IClipRepository
{
    private readonly string _storagePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonStorageProvider()
    {
        _storagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Anis");
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    public async Task<AppSettings> LoadAsync()
    {
        var settingsFile = Path.Combine(_storagePath, "settings.json");
        if (!File.Exists(settingsFile)) return new AppSettings();
        try
        {
            using var stream = File.OpenRead(settingsFile);
            return await JsonSerializer.DeserializeAsync<AppSettings>(stream) ?? new AppSettings();
        }
        catch (JsonException) { return new AppSettings(); }
    }

    public async Task SaveAsync(AppSettings settings)
    {
        var settingsFile = Path.Combine(_storagePath, "settings.json");
        using var stream = File.Create(settingsFile);
        await JsonSerializer.SerializeAsync(stream, settings, _jsonOptions);
    }

    public Task<IEnumerable<Clip>> GetClipsAsync() => LoadDataAsync<Clip>("clips.json");
    public Task<IEnumerable<Reciter>> GetRecitersAsync() => LoadDataAsync<Reciter>("reciters.json");

    // This is the new method for themes
    public Task<IEnumerable<Theme>> GetThemesAsync() => LoadDataAsync<Theme>("themes.json");

    private async Task<IEnumerable<T>> LoadDataAsync<T>(string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);
        if (!File.Exists(filePath)) return Enumerable.Empty<T>();
        try
        {
            using var stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<List<T>>(stream) ?? Enumerable.Empty<T>();
        }
        catch (JsonException) { return Enumerable.Empty<T>(); }
    }
    public async Task SaveClipsAsync(IEnumerable<Clip> clips)
    {
        var clipsFile = Path.Combine(_storagePath, "clips.json");
        using var stream = File.Create(clipsFile);
        await JsonSerializer.SerializeAsync(stream, clips, _jsonOptions);
    }
}