using DiabetesHelper.Models;

namespace DiabetesHelper.Services;

// The service MealPhotoViewModel actually depends on. Looks up the single globally-active key
// entry and delegates to that entry's provider's IFoodVisionService implementation, so
// "Analyze Photo" always uses the active provider/key rather than a hardcoded one.
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
        var active = await _apiKeyVault.GetActiveEntryAsync();
        if (active is null)
        {
            throw new FoodVisionException("No AI provider is active yet. Pick one in Settings and save it.");
        }

        if (!_servicesByProvider.TryGetValue(active.Provider, out var service))
        {
            throw new FoodVisionException($"Unknown AI provider '{active.Provider}'. Pick one in Settings.");
        }

        return await service.AnalyzeMealPhotoAsync(imageBytes, mediaType, cancellationToken);
    }
}
