namespace DiabetesHelper.Services;

// Shared instructions given to every vision-capable model, regardless of provider - keeps
// behavior (and the expected JSON response shape) consistent no matter which one is active.
internal static class FoodVisionPrompt
{
    public const string Text = """
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
}
