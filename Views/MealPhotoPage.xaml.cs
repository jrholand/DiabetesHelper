using DiabetesHelper.ViewModels;

namespace DiabetesHelper.Views;

public partial class MealPhotoPage : ContentPage
{
    public MealPhotoPage(MealPhotoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnSettingsToolbarItemClicked(object? sender, EventArgs e) => await SettingsNavigation.ShowAsync();
}
