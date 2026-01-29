using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using _10xDevs_CodeReviewChecklistGenerator.Data;

namespace _10xDevs_CodeReviewChecklistGenerator.Pages.Checklists;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<ChecklistListItem> Checklists { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        Checklists = await _db.Checklists
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ChecklistListItem
            {
                Id = c.Id,
                Title = c.Title,
                CreatedAt = c.CreatedAt,
                ItemCount = c.Items.Count
            })
            .ToListAsync(ct);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var checklist = await _db.Checklists.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);
        if (checklist is null)
        {
            return RedirectToPage();
        }

        _db.Checklists.Remove(checklist);
        await _db.SaveChangesAsync(ct);
        return RedirectToPage();
    }

    public sealed class ChecklistListItem
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
        public int ItemCount { get; init; }
    }
}
