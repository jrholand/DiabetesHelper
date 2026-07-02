using DiabetesHelper.Models;

namespace DiabetesHelper.Services;

// The service MealPhotoViewModel actually depends on. Looks up whichever provider is currently
// selected (Settings screen) and delegates to that provider's IFoodVisionService implementation,
// so "Analyze Photo" always uses the active provider/key rather than a hardcoded one.
public class ActiveProviderFoodVisionService : IFoodVisionService
{
    private readonly IApiKeyVaultService _apiKeyVault;
    private readonly IReadOnlyDictionary<string, IFoodVisionService> _servicesByProvider;

    public ActiveProviderFoodVisionService(
        IApiKeyVaultService apiKeyVault,
        AnthropicFoodVisionService anthropic,
        OpenAiFoodVisionService openAi,
        GoogleGeminiFoodVisionService googleGemini,
        MistralFoodVisionService mistral,
        XaiGrokFoodVisionService xaiGrok)
    {
        _apiKeyVault = apiKeyVault;
        _servicesByProvider = new Dictionary<string, IFoodVisionService>
        {
            [AiProviders.Anthropic] = anthropic,
            [AiProviders.OpenAi] = openAi,
            [AiProviders.GoogleGemini] = googleGemini,
            [AiProviders.MistralAi] = mistral,
            [AiProviders.XaiGrok] = xaiGrok
        };
    }

    public async Task<List<FoodItemEstimate>> AnalyzeMealPhotoAsync(byte[] imageBytes, string mediaType, CancellationToken cancellationToken)
    {
        var provider = await _apiKeyVault.GetSelectedProviderAsync();
        if (!_servicesByProvider.TryGetValue(provider, out var service))
        {
            throw new FoodVisionException($"Unknown AI provider '{provider}'. Pick one in Settings.");
        }

        return await service.AnalyzeMealPhotoAsync(imageBytes, mediaType, cancellationToken);
    }
}
