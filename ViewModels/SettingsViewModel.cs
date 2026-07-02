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

    // Whether saving the key in ApiKey below should also make it the one globally-active
    // entry, versus just adding it to that provider's history without switching what's active.
    // Defaults on since that's the common case - entering a key almost always means "use it now".
    [ObservableProperty]
    private bool makeActiveProvider = true;

    public SettingsViewModel(IApiKeyVaultService apiKeyVault)
    {
        _apiKeyVault = apiKeyVault;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var active = await _apiKeyVault.GetActiveEntryAsync();
        SelectedProvider = AvailableProviders.FirstOrDefault(p => p.Id == active?.Provider) ?? AvailableProviders[0];

        ApiKey = string.Empty;
        StatusMessage = null;
        await RefreshHistoryAsync();
    }

    // Changing the picker only switches which provider's key/history you're viewing here -
    // it does NOT by itself change which provider the app uses. That's controlled by the
    // "Save Provider" button, the MakeActiveProvider checkbox on save, or "Use this key" below.
    [RelayCommand]
    private async Task ProviderChangedAsync()
    {
        ApiKey = string.Empty;
        StatusMessage = null;
        await RefreshHistoryAsync();
    }

    // Makes SelectedProvider's most-recently-entered key the one globally-active entry,
    // without requiring the user to add a new key just to switch back to a provider they've
    // already saved a key for.
    [RelayCommand]
    private async Task SaveProviderAsync()
    {
        var mostRecent = KeyHistory.FirstOrDefault();
        if (mostRecent is null)
        {
            StatusMessage = "Add a key for this provider first.";
            return;
        }

        await _apiKeyVault.ActivateAsync(mostRecent.Id);
        StatusMessage = $"{SelectedProvider.DisplayName} is now the active provider.";
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

        var newEntry = await _apiKeyVault.AddKeyAsync(SelectedProvider.Id, ApiKey.Trim());
        ApiKey = string.Empty;

        if (MakeActiveProvider)
        {
            await _apiKeyVault.ActivateAsync(newEntry.Id);
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
        await _apiKeyVault.ActivateAsync(entry.Id);
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
