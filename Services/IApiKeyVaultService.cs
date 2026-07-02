using DiabetesHelper.Models;

namespace DiabetesHelper.Services;

// Provider-aware, history-preserving store for AI API keys. Every key ever entered is kept
// (with the date/time it was entered) so the user can switch back to an older key for a
// provider without re-typing it. Exactly one entry, across ALL providers, is ever "active" -
// that entry's Provider is which provider ActiveProviderFoodVisionService uses.
public interface IApiKeyVaultService
{
    // All entries for a provider, newest first. Each entry's IsActive reflects whether it is
    // THE single globally-active entry, not merely "was once active for this provider".
    Task<List<ApiKeyEntry>> GetHistoryAsync(string provider);

    // The one globally-active entry across all providers, or null if none has been activated yet.
    Task<ApiKeyEntry?> GetActiveEntryAsync();

    // The active entry's key, but only if it belongs to the given provider; null otherwise
    // (including when no entry is active at all).
    Task<string?> GetActiveKeyAsync(string provider);

    // Adds a new entry for the provider. Does NOT change which entry is globally active.
    Task<ApiKeyEntry> AddKeyAsync(string provider, string key);

    // Makes entryId the one globally-active entry, deactivating whatever was active before -
    // regardless of which provider it belonged to.
    Task ActivateAsync(int entryId);
}
