using System.Text.Json;
using DiabetesHelper.Models;

namespace DiabetesHelper.Services;

// Every provider is prompted (see FoodVisionPrompt) to respond with the same bare JSON array,
// so all of them share this same extraction/parsing path regardless of provider.
internal static class FoodVisionResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static List<FoodItemEstimate> ParseFoodItems(string text)
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

    private class FoodItemJson
    {
        public string Name { get; set; } = string.Empty;

        public double CarbsGrams { get; set; }

        public int? GlycemicIndex { get; set; }
    }
}
