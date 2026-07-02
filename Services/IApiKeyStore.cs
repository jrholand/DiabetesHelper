namespace DiabetesHelper.Services;

// Thin seam over SecureStorage so the Settings page (writer) and vision services
// (readers) don't each hardcode SecureStorage key names.
public interface IApiKeyStore
{
    Task<string?> GetAnthropicApiKeyAsync();

    Task SaveAnthropicApiKeyAsync(string apiKey);

    Task ClearAnthropicApiKeyAsync();
}
