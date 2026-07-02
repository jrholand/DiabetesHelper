namespace DiabetesHelper.Services;

public class OpenAiFoodVisionService : OpenAiCompatibleFoodVisionService
{
    public OpenAiFoodVisionService(HttpClient httpClient, IApiKeyVaultService apiKeyVault)
        : base(httpClient, apiKeyVault, new Uri("https://api.openai.com/v1/"), AiProviders.OpenAi, "OpenAI", "gpt-4o")
    {
    }
}
