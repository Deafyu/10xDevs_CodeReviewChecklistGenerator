namespace _10xDevs_CodeReviewChecklistGenerator.Api;

public sealed class ChecklistGenerateRequest
{
    public string CodeBefore { get; init; } = string.Empty;
    public string CodeAfter { get; init; } = string.Empty;
    public string ChangeDescription { get; init; } = string.Empty;
    public Guid? TemplateId { get; init; }
}

public sealed class ChecklistGenerateResponse
{
    public List<string> Items { get; init; } = [];
}

public sealed class ChecklistCreateRequest
{
    public string Title { get; init; } = string.Empty;
    public string CodeBefore { get; init; } = string.Empty;
    public string CodeAfter { get; init; } = string.Empty;
    public string ChangeDescription { get; init; } = string.Empty;
    public List<ChecklistItemDto> Items { get; init; } = [];
}

public sealed class ChecklistItemDto
{
    public Guid? Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public bool IsChecked { get; init; }
    public int SortOrder { get; init; }
}
