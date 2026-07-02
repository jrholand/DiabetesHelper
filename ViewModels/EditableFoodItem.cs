using CommunityToolkit.Mvvm.ComponentModel;

namespace DiabetesHelper.ViewModels;

// Not a SQLite model - a row in the Meal Photo page's editable results list, letting the
// user correct an AI-estimated carb/GI value before it's saved and used for dosing decisions.
public partial class EditableFoodItem : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private double carbsGrams;

    [ObservableProperty]
    private int? glycemicIndex;
}
