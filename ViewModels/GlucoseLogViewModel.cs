using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiabetesHelper.Models;
using DiabetesHelper.Services;

namespace DiabetesHelper.ViewModels;

public partial class GlucoseLogViewModel : ObservableObject
{
    private readonly IRecordRepository<GlucoseReading> _repository;

    public ObservableCollection<GlucoseReading> Readings { get; } = new();

    [ObservableProperty]
    private double newValueMgDl;

    [ObservableProperty]
    private string? newNotes;

    public GlucoseLogViewModel(IRecordRepository<GlucoseReading> repository)
    {
        _repository = repository;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        Readings.Clear();
        var items = await _repository.GetAllAsync();
        foreach (var item in items.OrderByDescending(r => r.EffectiveDateUtc))
        {
            Readings.Add(item);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (NewValueMgDl <= 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var reading = new GlucoseReading
        {
            CreatedAtUtc = now,
            EffectiveDateUtc = now,
            ValueMgDl = NewValueMgDl,
            Notes = NewNotes
        };

        await _repository.SaveAsync(reading);
        NewValueMgDl = 0;
        NewNotes = null;
        await LoadAsync();
    }
}
