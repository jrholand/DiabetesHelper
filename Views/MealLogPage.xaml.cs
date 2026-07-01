using DiabetesHelper.ViewModels;

namespace DiabetesHelper.Views;

public partial class MealLogPage : ContentPage
{
    private readonly MealLogViewModel _viewModel;

    public MealLogPage(MealLogViewModel viewModel)
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
