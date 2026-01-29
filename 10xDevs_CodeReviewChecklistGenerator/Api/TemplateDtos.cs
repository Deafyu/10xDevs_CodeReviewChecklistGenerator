namespace _10xDevs_CodeReviewChecklistGenerator.Api;

public sealed class TemplateCreateRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<TemplateItemDto> Items { get; init; } = [];
}

public sealed class TemplateItemDto
{
    public Guid? Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public int SortOrder { get; init; }
}
