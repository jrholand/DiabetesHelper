namespace DiabetesHelper.Services;

// A provider offered in the Settings picker. ToString() drives display when a control shows
// the item's default string representation (e.g. the picked value on some platforms).
public record ProviderOption(string Id, string DisplayName)
{
    public override string ToString() => DisplayName;
}

// Fixed catalog of major hosted AI providers the Settings screen lets users configure a key for.
public static class AiProviders
{
    public const string Anthropic = "anthropic";
    public const string OpenAi = "openai";
    public const string GoogleGemini = "google_gemini";
    public const string MistralAi = "mistral";
    public const string XaiGrok = "xai_grok";

    public static readonly IReadOnlyList<ProviderOption> All = new List<ProviderOption>
    {
        new(Anthropic, "Anthropic (Claude)"),
        new(OpenAi, "OpenAI (ChatGPT)"),
        new(GoogleGemini, "Google (Gemini)"),
        new(MistralAi, "Mistral AI"),
        new(XaiGrok, "xAI (Grok)"),
    };
}
