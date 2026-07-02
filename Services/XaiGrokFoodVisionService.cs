namespace DiabetesHelper.Services;

public class XaiGrokFoodVisionService : OpenAiCompatibleFoodVisionService
{
    public XaiGrokFoodVisionService(HttpClient httpClient, IApiKeyVaultService apiKeyVault)
        : base(httpClient, apiKeyVault, new Uri("https://api.x.ai/v1/"), AiProviders.XaiGrok, "xAI (Grok)", "grok-2-vision-1212")
    {
    }
}
