namespace _10xDevs_CodeReviewChecklistGenerator.OpenRouter;

public sealed class OpenRouterOptions
{
    public const string SectionName = "OpenRouter";

    public string BaseUrl { get; init; } = "https://openrouter.ai/api/v1/";

    public string ApiKey { get; init; } = string.Empty;

    public string DefaultModel { get; init; } = "google/gemini-2.0-flash-exp:free";

    public string[] FallbackModels { get; init; } =
    [
        "deepseek/deepseek-chat-v3.1:free",
        "openai/gpt-oss-20b:free"
    ];

    public int? MaxTokens { get; init; } = 1000;

    public double? Temperature { get; init; } = 0.7;

    public string? HttpReferer { get; init; }

    public string? Title { get; init; }
}
