using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _10xDevs_CodeReviewChecklistGenerator.Data;
using _10xDevs_CodeReviewChecklistGenerator.Models;

namespace _10xDevs_CodeReviewChecklistGenerator.Pages.Templates;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public CreateModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var items = Input.ItemsText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select((text, index) => new ChecklistTemplateItem
            {
                Text = text,
                SortOrder = index
            })
            .ToList();

        var template = new ChecklistTemplate
        {
            UserId = userId,
            Name = Input.Name,
            Description = Input.Description,
            Items = items
        };

        _db.ChecklistTemplates.Add(template);
        await _db.SaveChangesAsync(ct);
        return RedirectToPage("/Templates/Index");
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
