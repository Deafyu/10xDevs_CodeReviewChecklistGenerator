using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using _10xDevs_CodeReviewChecklistGenerator.OpenRouter;
using _10xDevs_CodeReviewChecklistGenerator.OpenRouter.Models;
using _10xDevs_CodeReviewChecklistGenerator.Services;

namespace _10xDevs_CodeReviewChecklistGenerator.Tests;

public class OpenRouterClientTests
{
    [Fact]
    public async Task GetCompletionAsync_ReturnsMessageContent()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new ChatCompletionResponse
            {
                Choices =
                [
                    new Choice
                    {
                        Message = new ChatMessage
                        {
                            Role = "assistant",
                            Content = "hello"
                        }
                    }
                ]
            })
        });

        var client = CreateClient(handler, apiKey: "test-key");

        var result = await client.GetCompletionAsync("ping");

        Assert.Equal("hello", result);
        Assert.Equal("Bearer", handler.Request!.Headers.Authorization?.Scheme);
        Assert.Equal("test-key", handler.Request!.Headers.Authorization?.Parameter);
        Assert.EndsWith("/chat/completions", handler.Request!.RequestUri!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetCompletionAsync_T_ReturnsDeserializedObject()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new ChatCompletionResponse
            {
                Choices =
                [
                    new Choice
                    {
                        Message = new ChatMessage
                        {
                            Role = "assistant",
                            Content = """{"answer":"ok"}"""
                        }
                    }
                ]
            })
        });

        var client = CreateClient(handler);

        var result = await client.GetCompletionAsync<TestDto>("ping");

        Assert.NotNull(result);
        Assert.Equal("ok", result!.Answer);

        var body = JsonDocument.Parse(handler.RequestBody!);
        Assert.Equal("json_object", body.RootElement.GetProperty("response_format").GetProperty("type").GetString());
    }

    private static OpenRouterClient CreateClient(RecordingHandler handler, string? apiKey = null)
    {
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new OpenRouterOptions
        {
            BaseUrl = "https://openrouter.ai/api/v1/",
            ApiKey = apiKey ?? string.Empty,
            DefaultModel = "test-model",
            MaxTokens = 100,
            Temperature = 0.1,
            HttpReferer = "https://example.test",
            Title = "Test"
        });

        return new OpenRouterClient(httpClient, options, NullLogger<OpenRouterClient>.Instance);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        public HttpRequestMessage? Request { get; private set; }

        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            if (request.Content is not null)
            {
                RequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return _responseFactory(request);
        }
    }

    private sealed record TestDto(string Answer);
}
