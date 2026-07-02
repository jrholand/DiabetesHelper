namespace DiabetesHelper.Services;

public class MistralFoodVisionService : OpenAiCompatibleFoodVisionService
{
    public MistralFoodVisionService(HttpClient httpClient, IApiKeyVaultService apiKeyVault)
        : base(httpClient, apiKeyVault, new Uri("https://api.mistral.ai/v1/"), AiProviders.MistralAi, "Mistral AI", "pixtral-large-latest")
    {
    }
}
