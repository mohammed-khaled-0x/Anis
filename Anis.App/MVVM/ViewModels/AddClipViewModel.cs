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
    [ObservableProperty]
    private bool _isEditMode = false;
    private string? _editingClipId;

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
    private async Task SaveClipAsync(Window? window) // Made Window nullable to be safe
    {
        // --- VALIDATION ---
        // In edit mode, we don't require a new file path.
        if (string.IsNullOrWhiteSpace(ClipText) ||
            string.IsNullOrWhiteSpace(ClipTitle) ||
            SelectedReciter == null)
        {
            MessageBox.Show("الرجاء ملء حقول العنوان والنص واختيار قارئ وملف صوتي.", "بيانات ناقصة", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // First, load all the clips that currently exist.
            var allClips = (await _clipRepository.GetClipsAsync()).ToList();

            if (IsEditMode)
            {
                // --- EDIT LOGIC ---
                // We are in Edit Mode. Find the existing clip by its ID.
                var clipToUpdate = allClips.FirstOrDefault(c => c.Id == _editingClipId);
                if (clipToUpdate != null)
                {
                    // Update the properties of the found clip with the new data from the UI.
                    clipToUpdate.Title = ClipTitle;
                    clipToUpdate.Text = ClipText;
                    clipToUpdate.ReciterId = SelectedReciter.Id;
                    // Note: We are not changing the audio file itself in edit mode for simplicity.
                    // The user would need to delete and re-add to change the file.
                }
                else
                {
                    MessageBox.Show("لم يتم العثور على المقطع المراد تعديله. قد يكون تم حذفه.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                string? relativeFilePath = null; // Default to null

                // Only copy a file if one was selected
                if (!string.IsNullOrWhiteSpace(SourceFilePath))
                {
                    var newId = Guid.NewGuid().ToString("N");
                    var extension = Path.GetExtension(SourceFilePath);
                    var newFileName = $"{newId}{extension}";
                    var destinationPath = Path.Combine(_clipsStoragePath, newFileName);

                    File.Copy(SourceFilePath, destinationPath, true);
                    relativeFilePath = Path.Combine("clips", newFileName);
                }

                var newClip = new Clip
                {
                    Id = Guid.NewGuid().ToString("N"), // Always generate a new ID
                    ReciterId = SelectedReciter.Id,
                    Text = ClipText,
                    Title = ClipTitle,
                    FilePath = relativeFilePath, // Can be null now
                    IsEnabled = true,
                    Tags = new List<string>()
                };

                allClips.Add(newClip);
            }

            await _clipRepository.SaveClipsAsync(allClips);

            // Tell the scheduler to reload its data to reflect the changes immediately.
            await _scheduler.RefreshDataAsync();

            MessageBox.Show("تم الحفظ بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

            // This is important for the calling window to know the operation succeeded.
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void SetClipForEditing(Clip clip)
    {
        IsEditMode = true;
        _editingClipId = clip.Id;

        ClipTitle = clip.Title;
        ClipText = clip.Text;
        SelectedReciter = AvailableReciters.FirstOrDefault(r => r.Id == clip.ReciterId);
        SourceFilePath = "لا يمكن تغيير الملف الصوتي حاليًا. لحذفه وإضافة واحد جديد."; // Simplification for now
    }

}
