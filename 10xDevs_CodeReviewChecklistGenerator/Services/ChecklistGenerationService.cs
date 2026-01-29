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
        bool compareMode = true,
        CancellationToken ct = default)
    {
        var templateBlock = templateItems.Any()
            ? string.Join("\n", templateItems.Select(item => $"- {item}"))
            : "None";

        var prompt = compareMode
            ? BuildComparePrompt(codeBefore, codeAfter, changeDescription, templateBlock)
            : BuildSinglePrompt(codeAfter, changeDescription, templateBlock);

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

    private static string BuildComparePrompt(
        string codeBefore,
        string codeAfter,
        string changeDescription,
        string templateBlock)
    {
        var changesOnly = BuildChangedLinesOnly(codeBefore, codeAfter);

        return $"""
Generate a concise code review checklist focused only on the lines that changed.
Return a JSON object with a single field "items" containing an array of strings.

Change description:
{changeDescription}

Changed lines (before -> after):
{changesOnly}

Template items to include or expand:
{templateBlock}
""";
    }

    private static string BuildSinglePrompt(
        string code,
        string changeDescription,
        string templateBlock)
    {
        return $"""
Generate a concise code review checklist for the single code block below.
Return a JSON object with a single field "items" containing an array of strings.

Change description:
{changeDescription}

Code:
{code}

Template items to include or expand:
{templateBlock}
""";
    }

    private static string BuildChangedLinesOnly(string codeBefore, string codeAfter)
    {
        var beforeLines = codeBefore.Split('\n');
        var afterLines = codeAfter.Split('\n');
        var max = Math.Max(beforeLines.Length, afterLines.Length);
        var changes = new List<string>();

        for (var i = 0; i < max; i++)
        {
            var beforeLine = i < beforeLines.Length ? beforeLines[i] : string.Empty;
            var afterLine = i < afterLines.Length ? afterLines[i] : string.Empty;

            if (!string.Equals(beforeLine, afterLine, StringComparison.Ordinal))
            {
                changes.Add($"L{i + 1} - {beforeLine}");
                changes.Add($"L{i + 1} + {afterLine}");
            }
        }

        return changes.Count == 0 ? "No changes detected." : string.Join("\n", changes);
    }
}
