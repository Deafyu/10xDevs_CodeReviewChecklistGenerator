using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using _10xDevs_CodeReviewChecklistGenerator.OpenRouter.Models;

namespace _10xDevs_CodeReviewChecklistGenerator.OpenRouter;

public interface IOpenRouterClient
{
    Task<string> GetCompletionAsync(
        string prompt,
        string? systemPrompt = null,
        CancellationToken ct = default);

    Task<T?> GetCompletionAsync<T>(
        string prompt,
        string? systemPrompt = null,
        CancellationToken ct = default) where T : class;
}

public sealed class OpenRouterClient : IOpenRouterClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly OpenRouterOptions _options;
    private readonly ILogger<OpenRouterClient> _logger;

    public OpenRouterClient(
        HttpClient httpClient,
        IOptions<OpenRouterOptions> options,
        ILogger<OpenRouterClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GetCompletionAsync(
        string prompt,
        string? systemPrompt = null,
        CancellationToken ct = default)
    {
        var request = BuildRequest(prompt, systemPrompt, includeResponseFormat: false);
        var response = await SendWithFallbackAsync(request, ct);

        var result = await response.Content
            .ReadFromJsonAsync<ChatCompletionResponse>(JsonOptions, ct);

        return result?.Choices.FirstOrDefault()?.Message.Content
            ?? throw new InvalidOperationException("Empty response from OpenRouter.");
    }

    public async Task<T?> GetCompletionAsync<T>(
        string prompt,
        string? systemPrompt = null,
        CancellationToken ct = default) where T : class
    {
        var request = BuildRequest(prompt, systemPrompt, includeResponseFormat: true);
        var response = await SendWithFallbackAsync(request, ct);

        var result = await response.Content
            .ReadFromJsonAsync<ChatCompletionResponse>(JsonOptions, ct);

        var content = result?.Choices.FirstOrDefault()?.Message.Content;
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(content, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize OpenRouter response to {Type}.", typeof(T).Name);
            return null;
        }
    }

    private ChatCompletionRequest BuildRequest(
        string prompt,
        string? systemPrompt,
        bool includeResponseFormat)
    {
        var messages = new List<ChatMessage>();

        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            messages.Add(new ChatMessage { Role = "system", Content = systemPrompt });
        }

        messages.Add(new ChatMessage { Role = "user", Content = prompt });

        return new ChatCompletionRequest
        {
            Model = _options.DefaultModel,
            Messages = messages,
            MaxTokens = _options.MaxTokens,
            Temperature = _options.Temperature,
            ResponseFormat = includeResponseFormat ? new ResponseFormat { Type = "json_object" } : null
        };
    }

    private async Task<HttpResponseMessage> SendWithFallbackAsync(
        ChatCompletionRequest request,
        CancellationToken ct)
    {
        var models = new List<string> { request.Model };
        if (_options.FallbackModels is { Length: > 0 })
        {
            models.AddRange(_options.FallbackModels.Where(model =>
                !string.IsNullOrWhiteSpace(model) &&
                !string.Equals(model, request.Model, StringComparison.OrdinalIgnoreCase)));
        }

        HttpResponseMessage? lastResponse = null;

        foreach (var model in models)
        {
            var attempt = request with { Model = model };
            var response = await SendOnceAsync(attempt, ct);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("OpenRouter model not found: {Model}. Trying fallback.", model);
                lastResponse = response;
                continue;
            }

            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("OpenRouter request failed. Status: {StatusCode}, Body: {Body}", response.StatusCode, body);
            response.EnsureSuccessStatusCode();
        }

        if (lastResponse is not null)
        {
            var body = await lastResponse.Content.ReadAsStringAsync(ct);
            _logger.LogError("OpenRouter request failed after fallbacks. Status: {StatusCode}, Body: {Body}", lastResponse.StatusCode, body);
            lastResponse.EnsureSuccessStatusCode();
        }

        throw new InvalidOperationException("OpenRouter request failed and no fallback succeeded.");
    }

    private async Task<HttpResponseMessage> SendOnceAsync(
        ChatCompletionRequest request,
        CancellationToken ct)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri(new Uri(_options.BaseUrl), "chat/completions"))
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        if (!string.IsNullOrWhiteSpace(_options.HttpReferer))
        {
            message.Headers.Add("HTTP-Referer", _options.HttpReferer);
        }

        if (!string.IsNullOrWhiteSpace(_options.Title))
        {
            message.Headers.Add("X-Title", _options.Title);
        }

        return await _httpClient.SendAsync(message, ct);
    }
}
