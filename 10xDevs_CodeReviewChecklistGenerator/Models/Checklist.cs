namespace _10xDevs_CodeReviewChecklistGenerator.Models;

public class Checklist
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CodeBefore { get; set; } = string.Empty;
    public string CodeAfter { get; set; } = string.Empty;
    public string ChangeDescription { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<ChecklistItem> Items { get; set; } = new List<ChecklistItem>();
}
