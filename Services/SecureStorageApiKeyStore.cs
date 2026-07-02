namespace DiabetesHelper.Services;

public class SecureStorageApiKeyStore : IApiKeyStore
{
    private const string AnthropicApiKeyName = "anthropic_api_key";

    public Task<string?> GetAnthropicApiKeyAsync() =>
        SecureStorage.Default.GetAsync(AnthropicApiKeyName);

    public Task SaveAnthropicApiKeyAsync(string apiKey) =>
        SecureStorage.Default.SetAsync(AnthropicApiKeyName, apiKey);

    public Task ClearAnthropicApiKeyAsync()
    {
        SecureStorage.Default.Remove(AnthropicApiKeyName);
        return Task.CompletedTask;
    }
}
