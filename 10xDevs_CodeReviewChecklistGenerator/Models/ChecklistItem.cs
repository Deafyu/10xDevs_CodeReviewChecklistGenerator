namespace _10xDevs_CodeReviewChecklistGenerator.Models;

public class ChecklistItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChecklistId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsChecked { get; set; }
    public int SortOrder { get; set; }

    public Checklist? Checklist { get; set; }
}
