using _10xDevs_CodeReviewChecklistGenerator.OpenRouter;
using _10xDevs_CodeReviewChecklistGenerator.Services;

namespace _10xDevs_CodeReviewChecklistGenerator.Tests;

public class ChecklistGenerationServiceTests
{
    [Fact]
    public async Task GenerateAsync_ReturnsDistinctTrimmedItems()
    {
        var client = new StubOpenRouterClient(new ChecklistAiResponse
        {
            Items = ["  Item A  ", "item a", "Item B"]
        });
        var service = new ChecklistGenerationService(client);

        var result = await service.GenerateAsync("before", "after", "desc", []);

        Assert.Equal(["Item A", "Item B"], result);
    }

    [Fact]
    public async Task GenerateAsync_ThrowsWhenNoItems()
    {
        var client = new StubOpenRouterClient(new ChecklistAiResponse());
        var service = new ChecklistGenerationService(client);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GenerateAsync("before", "after", "desc", []));
    }

    private sealed class StubOpenRouterClient : IOpenRouterClient
    {
        private readonly ChecklistAiResponse _response;

        public StubOpenRouterClient(ChecklistAiResponse response)
        {
            _response = response;
        }

        public Task<string> GetCompletionAsync(string prompt, string? systemPrompt = null, CancellationToken ct = default)
        {
            throw new NotSupportedException();
        }

        public Task<T?> GetCompletionAsync<T>(string prompt, string? systemPrompt = null, CancellationToken ct = default) where T : class
        {
            return Task.FromResult(_response as T);
        }
    }
}
