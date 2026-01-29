namespace _10xDevs_CodeReviewChecklistGenerator.Models;

public class ChecklistTemplateItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TemplateId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public ChecklistTemplate? Template { get; set; }
}
