using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiabetesHelper.Models;
using DiabetesHelper.Services;

namespace DiabetesHelper.ViewModels;

public partial class InsulinLogViewModel : ObservableObject
{
    private readonly IRecordRepository<InsulinDose> _repository;

    public ObservableCollection<InsulinDose> Doses { get; } = new();

    public IReadOnlyList<InsulinType> InsulinTypes { get; } = Enum.GetValues<InsulinType>();

    [ObservableProperty]
    private double newUnits;

    [ObservableProperty]
    private InsulinType newType = InsulinType.Bolus;

    [ObservableProperty]
    private string? newNotes;

    public InsulinLogViewModel(IRecordRepository<InsulinDose> repository)
    {
        _repository = repository;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        Doses.Clear();
        var items = await _repository.GetAllAsync();
        foreach (var item in items.OrderByDescending(d => d.TimestampUtc))
        {
            Doses.Add(item);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (NewUnits <= 0)
        {
            return;
        }

        var dose = new InsulinDose
        {
            TimestampUtc = DateTime.UtcNow,
            Units = NewUnits,
            Type = NewType,
            Notes = NewNotes
        };

        await _repository.SaveAsync(dose);
        NewUnits = 0;
        NewNotes = null;
        await LoadAsync();
    }
}
