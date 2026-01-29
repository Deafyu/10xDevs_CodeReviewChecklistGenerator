using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using _10xDevs_CodeReviewChecklistGenerator.Data;
using _10xDevs_CodeReviewChecklistGenerator.Models;

namespace _10xDevs_CodeReviewChecklistGenerator.Pages.Templates;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public EditModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        var template = await LoadTemplateAsync(id, ct);
        if (template is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Name = template.Name,
            Description = template.Description,
            ItemsText = string.Join('\n', template.Items.OrderBy(i => i.SortOrder).Select(i => i.Text))
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var template = await LoadTemplateAsync(id, ct);
        if (template is null)
        {
            return NotFound();
        }

        template.Name = Input.Name;
        template.Description = Input.Description;
        template.UpdatedAt = DateTimeOffset.UtcNow;
        template.Items.Clear();

        var items = Input.ItemsText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select((text, index) => new ChecklistTemplateItem
            {
                Text = text,
                SortOrder = index
            })
            .ToList();

        foreach (var item in items)
        {
            template.Items.Add(item);
        }

        await _db.SaveChangesAsync(ct);
        return RedirectToPage("/Templates/Index");
    }

    private async Task<ChecklistTemplate?> LoadTemplateAsync(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        return await _db.ChecklistTemplates
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);
    }

    public sealed class InputModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string ItemsText { get; set; } = string.Empty;
    }
}
