using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiabetesHelper.Services;

namespace DiabetesHelper.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IApiKeyStore _apiKeyStore;

    public List<string> AvailableProviders { get; } = new() { "Anthropic (Claude)" };

    [ObservableProperty]
    private string selectedProvider = "Anthropic (Claude)";

    [ObservableProperty]
    private string apiKey = string.Empty;

    [ObservableProperty]
    private string? statusMessage;

    public SettingsViewModel(IApiKeyStore apiKeyStore)
    {
        _apiKeyStore = apiKeyStore;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        ApiKey = await _apiKeyStore.GetAnthropicApiKeyAsync() ?? string.Empty;
        StatusMessage = null;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            StatusMessage = "Enter an API key first.";
            return;
        }

        await _apiKeyStore.SaveAnthropicApiKeyAsync(ApiKey.Trim());
        StatusMessage = "Saved.";
    }

    [RelayCommand]
    private async Task CloseAsync() => await Shell.Current.Navigation.PopModalAsync();
}
