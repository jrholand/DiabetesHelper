using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiabetesHelper.Models;
using DiabetesHelper.Services;

namespace DiabetesHelper.ViewModels;

public partial class EditGlucoseReadingViewModel : ObservableObject
{
    private readonly IRecordRepository<GlucoseReading> _repository;
    private GlucoseReading? _record;

    [ObservableProperty]
    private string summaryText = string.Empty;

    [ObservableProperty]
    private DateTime createdAtUtc;

    [ObservableProperty]
    private DateTime effectiveDate;

    [ObservableProperty]
    private TimeSpan effectiveTime;

    public EditGlucoseReadingViewModel(IRecordRepository<GlucoseReading> repository)
    {
        _repository = repository;
    }

    public void Load(GlucoseReading record)
    {
        _record = record;
        SummaryText = string.IsNullOrWhiteSpace(record.Notes)
            ? $"{record.ValueMgDl} mg/dL"
            : $"{record.ValueMgDl} mg/dL — {record.Notes}";
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
