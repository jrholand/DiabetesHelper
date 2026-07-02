using DiabetesHelper.ViewModels;

namespace DiabetesHelper.Views;

public partial class InsulinLogPage : ContentPage
{
    private readonly InsulinLogViewModel _viewModel;

    public InsulinLogPage(InsulinLogViewModel viewModel)
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

    private async void OnAboutToolbarItemClicked(object? sender, EventArgs e) => await AboutNavigation.ShowAsync();
}
