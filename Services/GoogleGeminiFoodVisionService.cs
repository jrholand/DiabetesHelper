using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DiabetesHelper.Models;

namespace DiabetesHelper.Services;

public class GoogleGeminiFoodVisionService : IFoodVisionService
{
    private const string ModelId = "gemini-2.0-flash";

    private readonly HttpClient _httpClient;
    private readonly IApiKeyVaultService _apiKeyVault;

    public GoogleGeminiFoodVisionService(HttpClient httpClient, IApiKeyVaultService apiKeyVault)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress ??= new Uri("https://generativelanguage.googleapis.com/v1beta/");
        _apiKeyVault = apiKeyVault;
    }

    public async Task<List<FoodItemEstimate>> AnalyzeMealPhotoAsync(byte[] imageBytes, string mediaType, CancellationToken cancellationToken)
    {
        var apiKey = await _apiKeyVault.GetActiveKeyAsync(AiProviders.GoogleGemini);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new FoodVisionException("No Google Gemini API key is configured. Add one in Settings.");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"models/{ModelId}:generateContent");
        request.Headers.Add("x-goog-api-key", apiKey);
        request.Content = JsonContent.Create(new
        {
            contents = new object[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = FoodVisionPrompt.Text },
                        new
                        {
                            inline_data = new
                            {
                                mime_type = mediaType,
                                data = Convert.ToBase64String(imageBytes)
                            }
                        }
                    }
                }
            }
        });

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new FoodVisionException("Couldn't reach the AI service. Check your connection and try again.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new FoodVisionException($"The AI service returned an error ({(int)response.StatusCode}). {body}");
        }

        var payload = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: cancellationToken);
        var text = payload?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new FoodVisionException("The AI service returned an empty response.");
        }

        return FoodVisionResponseParser.ParseFoodItems(text);
    }

    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    private class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart>? Parts { get; set; }
    }

    private class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
