using DiabetesHelper.Models;
using DiabetesHelper.ViewModels;

namespace DiabetesHelper.Views;

public partial class EditGlucoseReadingPage : ContentPage
{
    private readonly EditGlucoseReadingViewModel _viewModel;

    public EditGlucoseReadingPage(EditGlucoseReadingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public void Load(GlucoseReading record) => _viewModel.Load(record);
}
