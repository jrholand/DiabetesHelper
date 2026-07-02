using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiabetesHelper.Models;
using DiabetesHelper.Services;

namespace DiabetesHelper.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IApiKeyVaultService _apiKeyVault;

    public IReadOnlyList<ProviderOption> AvailableProviders { get; } = AiProviders.All;

    public ObservableCollection<ApiKeyEntry> KeyHistory { get; } = new();

    [ObservableProperty]
    private ProviderOption selectedProvider = AiProviders.All[0];

    // Always starts blank - this is a field for entering a NEW key, not an editor for the
    // active one. Prefilling it with the current active key would risk re-saving it as a
    // fresh, newly-timestamped history entry without the user having actually changed anything.
    [ObservableProperty]
    private string apiKey = string.Empty;

    [ObservableProperty]
    private string? statusMessage;

    // Whether saving the key in ApiKey below should also make SelectedProvider the one
    // ActiveProviderFoodVisionService actually uses, versus just adding it to that provider's
    // history without switching what's currently active. Defaults on since that's the common
    // case - entering a key almost always means "use it now".
    [ObservableProperty]
    private bool makeActiveProvider = true;

    public SettingsViewModel(IApiKeyVaultService apiKeyVault)
    {
        _apiKeyVault = apiKeyVault;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var selectedId = await _apiKeyVault.GetSelectedProviderAsync();
        SelectedProvider = AvailableProviders.FirstOrDefault(p => p.Id == selectedId) ?? AvailableProviders[0];

        ApiKey = string.Empty;
        StatusMessage = null;
        await RefreshHistoryAsync();
    }

    // Changing the picker only switches which provider's key/history you're viewing here -
    // it does NOT by itself change which provider the app uses. That's controlled by
    // MakeActiveProvider on save, or by explicitly picking "Use this key" below.
    [RelayCommand]
    private async Task ProviderChangedAsync()
    {
        ApiKey = string.Empty;
        StatusMessage = null;
        await RefreshHistoryAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            StatusMessage = "Enter an API key first.";
            return;
        }

        await _apiKeyVault.AddKeyAsync(SelectedProvider.Id, ApiKey.Trim());
        ApiKey = string.Empty;

        if (MakeActiveProvider)
        {
            await _apiKeyVault.SetSelectedProviderAsync(SelectedProvider.Id);
            StatusMessage = "Saved and set as the active provider.";
        }
        else
        {
            StatusMessage = "Saved.";
        }

        await RefreshHistoryAsync();
    }

    [RelayCommand]
    private async Task UseKeyAsync(ApiKeyEntry entry)
    {
        await _apiKeyVault.ActivateAsync(SelectedProvider.Id, entry.Id);
        await _apiKeyVault.SetSelectedProviderAsync(SelectedProvider.Id);
        StatusMessage = $"Now using the key entered {entry.EnteredLocal:g}.";
        await RefreshHistoryAsync();
    }

    [RelayCommand]
    private async Task CloseAsync() => await Shell.Current.Navigation.PopModalAsync();

    private async Task RefreshHistoryAsync()
    {
        var history = await _apiKeyVault.GetHistoryAsync(SelectedProvider.Id);
        KeyHistory.Clear();
        foreach (var entry in history)
        {
            KeyHistory.Add(entry);
        }
    }
}
