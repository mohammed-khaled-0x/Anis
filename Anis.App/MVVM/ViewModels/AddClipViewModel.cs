using Anis.Core.Domain;
using Anis.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Anis.App.MVVM.ViewModels;

public partial class AddClipViewModel : ObservableObject
{
    private readonly IClipRepository _clipRepository;
    private readonly string _clipsStoragePath;
    private readonly IScheduler _scheduler;

    [ObservableProperty] private string _clipText = "";
    [ObservableProperty] private string _clipTitle = "";
    [ObservableProperty] private string _sourceFilePath = "";
    [ObservableProperty] private List<Reciter> _availableReciters = new();
    [ObservableProperty] private Reciter? _selectedReciter;

    public AddClipViewModel(IClipRepository clipRepository, IScheduler scheduler)
    {
        _scheduler = scheduler;
        _clipRepository = clipRepository;
        _clipsStoragePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Anis", "clips");
        LoadReciters();
    }

    private async void LoadReciters()
    {
        AvailableReciters = (await _clipRepository.GetRecitersAsync()).ToList();
    }

    [RelayCommand]
    private void BrowseFile()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Audio Files (*.mp3;*.wav)|*.mp3;*.wav",
            Title = "اختر مقطعًا صوتيًا"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            SourceFilePath = openFileDialog.FileName;
        }
    }

    [RelayCommand]
    private async Task SaveClipAsync(Window window)
    {
        if (string.IsNullOrWhiteSpace(ClipText) || string.IsNullOrWhiteSpace(ClipTitle) ||
            string.IsNullOrWhiteSpace(SourceFilePath) || SelectedReciter == null)
        {
            MessageBox.Show("الرجاء ملء جميع الحقول واختيار ملف صوتي.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // 1. Generate a unique ID and new file name to prevent conflicts
            var newId = Guid.NewGuid().ToString("N");
            var extension = Path.GetExtension(SourceFilePath);
            var newFileName = $"{newId}{extension}";
            var destinationPath = Path.Combine(_clipsStoragePath, newFileName);

            // 2. Copy the file to the app's local storage
            File.Copy(SourceFilePath, destinationPath, true);

            // 3. Create the new Clip object
            var newClip = new Clip
            {
                Id = newId,
                ReciterId = SelectedReciter.Id,
                Text = ClipText,
                Title = ClipTitle,
                FilePath = Path.Combine("clips", newFileName), // Store relative path
                IsEnabled = true,
                Tags = new List<string>() // Add tags later if needed
            };

            // 4. Load existing clips, add the new one, and save back
            var allClips = (await _clipRepository.GetClipsAsync()).ToList();
            allClips.Add(newClip);
            await _clipRepository.SaveClipsAsync(allClips);

            await _scheduler.RefreshDataAsync();

            MessageBox.Show("تم حفظ المقطع بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            window?.Close(); // Close the dialog after saving
        }
        catch (Exception ex)
        {
            MessageBox.Show($"حدث خطأ أثناء حفظ المقطع: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
