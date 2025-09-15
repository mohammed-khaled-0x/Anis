using Anis.Core.Domain;
using Anis.Core.Interfaces;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Anis.Infrastructure.Storage;

public class JsonStorageProvider : ISettingsStore, IClipRepository
{
    private readonly string _storagePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonStorageProvider()
    {
        // This stores data in a safe, user-specific folder like C:\Users\YourUser\AppData\Local\Anis
        _storagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Anis");

        // Ensure the directory exists
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true // Makes the JSON file readable for humans
        };
    }

    // --- ISettingsStore Implementation ---

    public async Task<AppSettings> LoadAsync()
    {
        var settingsFile = Path.Combine(_storagePath, "settings.json");
        if (!File.Exists(settingsFile))
        {
            return new AppSettings(); // Return default settings if file doesn't exist
        }

        try
        {
            using var stream = File.OpenRead(settingsFile);
            var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream);
            return settings ?? new AppSettings();
        }
        catch (JsonException)
        {
            // If the file is corrupt, return default settings
            return new AppSettings();
        }
    }

    public async Task SaveAsync(AppSettings settings)
    {
        var settingsFile = Path.Combine(_storagePath, "settings.json");
        using var stream = File.Create(settingsFile);
        await JsonSerializer.SerializeAsync(stream, settings, _jsonOptions);
    }

    // --- IClipRepository Implementation ---

    public Task<IEnumerable<Clip>> GetClipsAsync()
    {
        return LoadDataAsync<Clip>("clips.json");
    }

    public Task<IEnumerable<Reciter>> GetRecitersAsync()
    {
        return LoadDataAsync<Reciter>("reciters.json");
    }

    // --- Helper Method ---

    private async Task<IEnumerable<T>> LoadDataAsync<T>(string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);
        if (!File.Exists(filePath))
        {
            return Enumerable.Empty<T>(); // Return an empty list if file doesn't exist
        }

        try
        {
            using var stream = File.OpenRead(filePath);
            var data = await JsonSerializer.DeserializeAsync<List<T>>(stream);
            return data ?? Enumerable.Empty<T>();
        }
        catch (JsonException)
        {
            // If file is corrupt, return an empty list
            return Enumerable.Empty<T>();
        }
    }
}