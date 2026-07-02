using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DiabetesHelper.Models;

namespace DiabetesHelper.Services;

public class AnthropicFoodVisionService : IFoodVisionService
{
    private const string ModelId = "claude-sonnet-5";
    private const string AnthropicVersion = "2023-06-01";

    private const string Prompt = """
        Identify each distinct food item visible in this photo of a meal. For each distinct
        item, estimate:
        - name: a short, common name for the food (e.g. "apple", "grilled chicken breast")
        - carbsGrams: your best estimate of total carbohydrates in grams for the visible
          portion of that item
        - glycemicIndex: your best estimate of the glycemic index (0-100) for that food, or
          null if you cannot reasonably estimate one

        Respond with ONLY a JSON array, no other text, no markdown code fences, no
        explanation. Example format:
        [{"name":"apple","carbsGrams":20,"glycemicIndex":36},{"name":"white rice","carbsGrams":45,"glycemicIndex":73}]

        If you cannot identify any food items, respond with an empty JSON array: []
        """;

    private readonly HttpClient _httpClient;
    private readonly IApiKeyStore _apiKeyStore;

    public AnthropicFoodVisionService(HttpClient httpClient, IApiKeyStore apiKeyStore)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress ??= new Uri("https://api.anthropic.com/");
        _apiKeyStore = apiKeyStore;
    }

    public async Task<List<FoodItemEstimate>> AnalyzeMealPhotoAsync(byte[] imageBytes, string mediaType, CancellationToken cancellationToken)
    {
        var apiKey = await _apiKeyStore.GetAnthropicApiKeyAsync();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new FoodVisionException("No Anthropic API key is configured. Add one in Settings.");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "v1/messages");
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", AnthropicVersion);
        request.Content = JsonContent.Create(new
        {
            model = ModelId,
            max_tokens = 1024,
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "image",
                            source = new
                            {
                                type = "base64",
                                media_type = mediaType,
                                data = Convert.ToBase64String(imageBytes)
                            }
                        },
                        new
                        {
                            type = "text",
                            text = Prompt
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

        var payload = await response.Content.ReadFromJsonAsync<AnthropicMessageResponse>(cancellationToken: cancellationToken);
        var text = payload?.Content?.FirstOrDefault(c => c.Type == "text")?.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new FoodVisionException("The AI service returned an empty response.");
        }

        return ParseFoodItems(text);
    }

    private static List<FoodItemEstimate> ParseFoodItems(string text)
    {
        var jsonArray = ExtractJsonArray(text);
        if (jsonArray is null)
        {
            throw new FoodVisionException("Couldn't understand the AI's response. Try another photo.");
        }

        List<FoodItemJson>? items;
        try
        {
            items = JsonSerializer.Deserialize<List<FoodItemJson>>(jsonArray, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new FoodVisionException("Couldn't understand the AI's response. Try another photo.", ex);
        }

        if (items is null)
        {
            return new List<FoodItemEstimate>();
        }

        return items
            .Where(i => !string.IsNullOrWhiteSpace(i.Name))
            .Select(i => new FoodItemEstimate
            {
                Name = i.Name.Trim(),
                CarbsGrams = i.CarbsGrams,
                GlycemicIndex = i.GlycemicIndex
            })
            .ToList();
    }

    // Models sometimes wrap output in markdown fences or a stray sentence despite the prompt's
    // instructions not to - tolerate that by taking the substring between the first '[' and last ']'.
    private static string? ExtractJsonArray(string text)
    {
        var start = text.IndexOf('[');
        var end = text.LastIndexOf(']');
        return start >= 0 && end > start ? text[start..(end + 1)] : null;
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private class AnthropicMessageResponse
    {
        [JsonPropertyName("content")]
        public List<AnthropicContentBlock>? Content { get; set; }
    }

    private class AnthropicContentBlock
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private class FoodItemJson
    {
        public string Name { get; set; } = string.Empty;

        public double CarbsGrams { get; set; }

        public int? GlycemicIndex { get; set; }
    }
}
