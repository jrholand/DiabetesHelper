using DiabetesHelper.Models;
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

    private async void OnAboutToolbarItemClicked(object? sender, EventArgs e) => await AboutNavigation.ShowAsync();

    private async void OnReadingSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not GlucoseReading reading)
        {
            return;
        }

        ((CollectionView)sender!).SelectedItem = null;
        await EditRecordNavigation.ShowGlucoseEditAsync(reading);
    }
}
