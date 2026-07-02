using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DiabetesHelper.Models;

namespace DiabetesHelper.Services;

// Shared implementation for providers exposing an OpenAI-compatible /chat/completions endpoint
// with image input as a data-URL "image_url" content part - OpenAI, Mistral, and xAI Grok all
// do, differing only in base address, model id, and which provider's stored key to use.
public abstract class OpenAiCompatibleFoodVisionService : IFoodVisionService
{
    private readonly HttpClient _httpClient;
    private readonly IApiKeyVaultService _apiKeyVault;
    private readonly string _providerId;
    private readonly string _providerDisplayName;
    private readonly string _modelId;

    protected OpenAiCompatibleFoodVisionService(
        HttpClient httpClient,
        IApiKeyVaultService apiKeyVault,
        Uri baseAddress,
        string providerId,
        string providerDisplayName,
        string modelId)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress ??= baseAddress;
        _apiKeyVault = apiKeyVault;
        _providerId = providerId;
        _providerDisplayName = providerDisplayName;
        _modelId = modelId;
    }

    public async Task<List<FoodItemEstimate>> AnalyzeMealPhotoAsync(byte[] imageBytes, string mediaType, CancellationToken cancellationToken)
    {
        var apiKey = await _apiKeyVault.GetActiveKeyAsync(_providerId);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new FoodVisionException($"No {_providerDisplayName} API key is configured. Add one in Settings.");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            model = _modelId,
            max_tokens = 1024,
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = FoodVisionPrompt.Text },
                        new
                        {
                            type = "image_url",
                            image_url = new { url = $"data:{mediaType};base64,{Convert.ToBase64String(imageBytes)}" }
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

        var payload = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken);
        var text = payload?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new FoodVisionException("The AI service returned an empty response.");
        }

        return FoodVisionResponseParser.ParseFoodItems(text);
    }

    private class ChatCompletionResponse
    {
        [JsonPropertyName("choices")]
        public List<ChatCompletionChoice>? Choices { get; set; }
    }

    private class ChatCompletionChoice
    {
        [JsonPropertyName("message")]
        public ChatCompletionMessage? Message { get; set; }
    }

    private class ChatCompletionMessage
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
