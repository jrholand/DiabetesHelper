using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiabetesHelper.Models;
using DiabetesHelper.Services;

namespace DiabetesHelper.ViewModels;

public partial class EditMealViewModel : ObservableObject
{
    private readonly IRecordRepository<Meal> _repository;
    private Meal? _record;

    [ObservableProperty]
    private string summaryText = string.Empty;

    [ObservableProperty]
    private DateTime createdAtUtc;

    [ObservableProperty]
    private DateTime effectiveDate;

    [ObservableProperty]
    private TimeSpan effectiveTime;

    public EditMealViewModel(IRecordRepository<Meal> repository)
    {
        _repository = repository;
    }

    public void Load(Meal record)
    {
        _record = record;
        SummaryText = $"{record.Description}, {record.CarbsGrams} g";
        CreatedAtUtc = record.CreatedAtUtc;

        var local = record.EffectiveDateUtc.ToLocalTime();
        EffectiveDate = local.Date;
        EffectiveTime = local.TimeOfDay;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_record is null)
        {
            return;
        }

        _record.EffectiveDateUtc = (EffectiveDate.Date + EffectiveTime).ToUniversalTime();
        await _repository.SaveAsync(_record);
        await Shell.Current.Navigation.PopModalAsync();
    }

    [RelayCommand]
    private async Task CancelAsync() => await Shell.Current.Navigation.PopModalAsync();
}
