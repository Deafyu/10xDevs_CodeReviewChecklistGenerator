using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using _10xDevs_CodeReviewChecklistGenerator.Data;
using _10xDevs_CodeReviewChecklistGenerator.Models;
using _10xDevs_CodeReviewChecklistGenerator.Services;

namespace _10xDevs_CodeReviewChecklistGenerator.Pages.Checklists;

[Authorize]
public class SingleModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ChecklistGenerationService _generator;

    public SingleModel(ApplicationDbContext db, ChecklistGenerationService generator)
    {
        _db = db;
        _generator = generator;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty]
    public List<ChecklistItemInput> GeneratedItems { get; set; } = [];

    public List<SelectListItem> TemplateOptions { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadTemplatesAsync(ct);
    }

    public async Task<IActionResult> OnPostGenerateAsync(CancellationToken ct)
    {
        await LoadTemplatesAsync(ct);
        ValidateInputs();
        if (!ModelState.IsValid)
            return Page();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var templateItems = new List<string>();
        if (Input.TemplateId.HasValue)
        {
            templateItems = await _db.ChecklistTemplateItems
                .Where(i => i.TemplateId == Input.TemplateId && i.Template!.UserId == userId)
                .OrderBy(i => i.SortOrder)
                .Select(i => i.Text)
                .ToListAsync(ct);
        }

        try
        {
            var generated = await _generator.GenerateAsync(
                string.Empty,
                Input.CodeOnly,
                Input.ChangeDescription,
                templateItems,
                false,
                ct);

            GeneratedItems = generated.Select((text, index) => new ChecklistItemInput
            {
                Text = text,
                SortOrder = index
            }).ToList();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty,
                $"AI generation failed: {ex.Message}. Check OpenRouter privacy settings or update the model.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct)
    {
        await LoadTemplatesAsync(ct);
        ValidateInputs();
        if (!ModelState.IsValid)
            return Page();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var checklist = new Checklist
        {
            UserId = userId,
            Title = Input.Title,
            CodeBefore = string.Empty,
            CodeAfter = Input.CodeOnly,
            ChangeDescription = Input.ChangeDescription,
            Items = GeneratedItems.Select((item, index) => new ChecklistItem
            {
                Text = item.Text,
                IsChecked = item.IsChecked,
                SortOrder = item.SortOrder == 0 ? index : item.SortOrder
            }).ToList()
        };

        _db.Checklists.Add(checklist);
        await _db.SaveChangesAsync(ct);
        return RedirectToPage("/Checklists/Details", new { id = checklist.Id });
    }

    private async Task LoadTemplatesAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        TemplateOptions = await _db.ChecklistTemplates
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .Select(t => new SelectListItem(t.Name, t.Id.ToString()))
            .ToListAsync(ct);
    }

    private void ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(Input.Title))
        {
            ModelState.AddModelError(nameof(Input.Title), "Title is required.");
        }

        if (string.IsNullOrWhiteSpace(Input.ChangeDescription))
        {
            ModelState.AddModelError(nameof(Input.ChangeDescription), "Change description is required.");
        }

        if (string.IsNullOrWhiteSpace(Input.CodeOnly))
        {
            ModelState.AddModelError(nameof(Input.CodeOnly), "Code to review is required.");
        }
    }

    public sealed class InputModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public Guid? TemplateId { get; set; }

        [Required]
        [Display(Name = "Code to review")]
        public string CodeOnly { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Change description")]
        public string ChangeDescription { get; set; } = string.Empty;
    }

    public sealed class ChecklistItemInput
    {
        public string Text { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
        public int SortOrder { get; set; }
    }
}
