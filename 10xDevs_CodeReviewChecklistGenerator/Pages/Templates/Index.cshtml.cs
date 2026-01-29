using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using _10xDevs_CodeReviewChecklistGenerator.Data;

namespace _10xDevs_CodeReviewChecklistGenerator.Pages.Templates;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<TemplateListItem> Templates { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        Templates = await _db.ChecklistTemplates
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TemplateListItem
            {
                Id = t.Id,
                Name = t.Name,
                ItemCount = t.Items.Count
            })
            .ToListAsync(ct);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var template = await _db.ChecklistTemplates.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);
        if (template is null)
        {
            return RedirectToPage();
        }

        _db.ChecklistTemplates.Remove(template);
        await _db.SaveChangesAsync(ct);
        return RedirectToPage();
    }

    public sealed class TemplateListItem
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int ItemCount { get; init; }
    }
}
