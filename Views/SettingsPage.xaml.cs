using DiabetesHelper.Models;
using DiabetesHelper.ViewModels;

namespace DiabetesHelper.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage(SettingsViewModel viewModel)
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

    private async void OnProviderSelectedIndexChanged(object? sender, EventArgs e) =>
        await _viewModel.ProviderChangedCommand.ExecuteAsync(null);

    private async void OnUseKeyClicked(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: ApiKeyEntry entry })
        {
            await _viewModel.UseKeyCommand.ExecuteAsync(entry);
        }
    }
}
