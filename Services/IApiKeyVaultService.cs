using DiabetesHelper.Models;

namespace DiabetesHelper.Services;

// Provider-aware, history-preserving store for AI API keys. Every key ever entered is kept
// (with the date/time it was entered) so the user can switch back to an older key for a
// provider without re-typing it; exactly one entry per provider is "active" at a time.
public interface IApiKeyVaultService
{
    // Newest first.
    Task<List<ApiKeyEntry>> GetHistoryAsync(string provider);

    Task<ApiKeyEntry?> GetActiveEntryAsync(string provider);

    Task<string?> GetActiveKeyAsync(string provider);

    // Adds a new entry for the provider, marks it active, and deactivates (without deleting)
    // any previously active entry for that same provider.
    Task<ApiKeyEntry> AddKeyAsync(string provider, string key);

    // Re-activates a previously entered key for the provider (a no-op on history - nothing
    // is deleted or re-dated).
    Task ActivateAsync(string provider, int entryId);

    Task<string> GetSelectedProviderAsync();

    Task SetSelectedProviderAsync(string provider);
}
