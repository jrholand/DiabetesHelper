using DiabetesHelper.ViewModels;

namespace DiabetesHelper.Views;

public partial class GlucoseLogPage : ContentPage
{
    private readonly GlucoseLogViewModel _viewModel;

    public GlucoseLogPage(GlucoseLogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}
