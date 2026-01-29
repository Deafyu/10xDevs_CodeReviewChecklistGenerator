using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using _10xDevs_CodeReviewChecklistGenerator.Data;

namespace _10xDevs_CodeReviewChecklistGenerator.Pages.Checklists;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DetailsModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public string Title { get; private set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; private set; }
    public Guid Id { get; private set; }

    [BindProperty]
    public List<ItemInput> Items { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        var checklist = await LoadChecklistAsync(id, ct);
        if (checklist is null)
        {
            return NotFound();
        }

        Title = checklist.Title;
        CreatedAt = checklist.CreatedAt;
        Id = checklist.Id;
        Items = checklist.Items
            .OrderBy(i => i.SortOrder)
            .Select(i => new ItemInput
            {
                Id = i.Id,
                Text = i.Text,
                IsChecked = i.IsChecked
            })
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken ct)
    {
        var checklist = await LoadChecklistAsync(id, ct);
        if (checklist is null)
        {
            return NotFound();
        }

        var itemMap = Items.ToDictionary(i => i.Id);
        foreach (var item in checklist.Items)
        {
            if (itemMap.TryGetValue(item.Id, out var input))
            {
                item.IsChecked = input.IsChecked;
            }
        }

        checklist.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return RedirectToPage(new { id });
    }

    private async Task<Models.Checklist?> LoadChecklistAsync(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        return await _db.Checklists
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);
    }

    public sealed class ItemInput
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
    }
}
