using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiabetesHelper.Models;
using DiabetesHelper.Services;

namespace DiabetesHelper.ViewModels;

public partial class MealLogViewModel : ObservableObject
{
    private readonly IRecordRepository<Meal> _repository;

    public ObservableCollection<Meal> Meals { get; } = new();

    [ObservableProperty]
    private string newDescription = string.Empty;

    [ObservableProperty]
    private double newCarbsGrams;

    [ObservableProperty]
    private string? newNotes;

    public MealLogViewModel(IRecordRepository<Meal> repository)
    {
        _repository = repository;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        Meals.Clear();
        var items = await _repository.GetAllAsync();
        foreach (var item in items.OrderByDescending(m => m.TimestampUtc))
        {
            Meals.Add(item);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(NewDescription))
        {
            return;
        }

        var meal = new Meal
        {
            TimestampUtc = DateTime.UtcNow,
            Description = NewDescription,
            CarbsGrams = NewCarbsGrams,
            Notes = NewNotes
        };

        await _repository.SaveAsync(meal);
        NewDescription = string.Empty;
        NewCarbsGrams = 0;
        NewNotes = null;
        await LoadAsync();
    }
}
