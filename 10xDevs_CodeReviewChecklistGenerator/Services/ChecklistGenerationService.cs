using _10xDevs_CodeReviewChecklistGenerator.OpenRouter;

namespace _10xDevs_CodeReviewChecklistGenerator.Services;

public sealed class ChecklistGenerationService
{
    private readonly IOpenRouterClient _client;

    public ChecklistGenerationService(IOpenRouterClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<string>> GenerateAsync(
        string codeBefore,
        string codeAfter,
        string changeDescription,
        IEnumerable<string> templateItems,
        CancellationToken ct = default)
    {
        var templateBlock = templateItems.Any()
            ? string.Join("\n", templateItems.Select(item => $"- {item}"))
            : "None";

        var prompt = $"""
Generate a concise code review checklist based on the change context.
Return a JSON object with a single field "items" containing an array of strings.

Change description:
{changeDescription}

Code before:
{codeBefore}

Code after:
{codeAfter}

Template items to include or expand:
{templateBlock}
""";

        var response = await _client.GetCompletionAsync<ChecklistAiResponse>(prompt, ct: ct);
        if (response?.Items is null || response.Items.Count == 0)
        {
            throw new InvalidOperationException("AI did not return any checklist items.");
        }

        return response.Items
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

}
