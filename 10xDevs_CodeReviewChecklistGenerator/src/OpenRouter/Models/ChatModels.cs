using System.Text.Json.Serialization;

namespace _10xDevs_CodeReviewChecklistGenerator.OpenRouter.Models;

public record ChatCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; init; } = [];

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; init; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; init; }

    [JsonPropertyName("response_format")]
    public ResponseFormat? ResponseFormat { get; init; }
}

public record ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;
}

public record ResponseFormat
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "json_object";
}

public record ChatCompletionResponse
{
    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; init; } = [];
}

public record Choice
{
    [JsonPropertyName("message")]
    public ChatMessage Message { get; init; } = new();
}
