using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using _10xDevs_CodeReviewChecklistGenerator.Api;
using _10xDevs_CodeReviewChecklistGenerator.Data;
using _10xDevs_CodeReviewChecklistGenerator.Models;
using _10xDevs_CodeReviewChecklistGenerator.OpenRouter;
using _10xDevs_CodeReviewChecklistGenerator.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddOptions<OpenRouterOptions>()
    .Bind(builder.Configuration.GetSection(OpenRouterOptions.SectionName));
builder.Services.AddHttpClient<IOpenRouterClient, OpenRouterClient>();
builder.Services.AddScoped<ChecklistGenerationService>();

var app = builder.Build();

if (string.IsNullOrWhiteSpace(app.Configuration["OpenRouter:ApiKey"]))
{
    app.Logger.LogWarning("OpenRouter ApiKey is missing. Set OpenRouter:ApiKey via user-secrets or environment variables.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

var api = app.MapGroup("/api").RequireAuthorization();

api.MapGet("/checklists", async (ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var items = await db.Checklists
        .AsNoTracking()
        .Where(c => c.UserId == userId)
        .OrderByDescending(c => c.CreatedAt)
        .Select(c => new
        {
            c.Id,
            c.Title,
            c.CreatedAt,
            c.UpdatedAt,
            ItemCount = c.Items.Count
        })
        .ToListAsync(ct);

    return Results.Ok(items);
});

api.MapGet("/checklists/{id:guid}", async (Guid id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var checklist = await db.Checklists
        .Include(c => c.Items)
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

    return checklist is null ? Results.NotFound() : Results.Ok(checklist);
});

api.MapPost("/checklists/generate", async (
    ChecklistGenerateRequest request,
    ApplicationDbContext db,
    ChecklistGenerationService generator,
    ClaimsPrincipal user,
    CancellationToken ct) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var templateItems = new List<string>();
    if (request.TemplateId.HasValue)
    {
        templateItems = await db.ChecklistTemplateItems
            .Where(i => i.TemplateId == request.TemplateId && i.Template!.UserId == userId)
            .OrderBy(i => i.SortOrder)
            .Select(i => i.Text)
            .ToListAsync(ct);
    }

    var items = await generator.GenerateAsync(
        request.CodeBefore,
        request.CodeAfter,
        request.ChangeDescription,
        templateItems,
        ct);

    return Results.Ok(new ChecklistGenerateResponse { Items = items.ToList() });
});

api.MapPost("/checklists", async (
    ChecklistCreateRequest request,
    ApplicationDbContext db,
    ClaimsPrincipal user,
    CancellationToken ct) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var checklist = new Checklist
    {
        UserId = userId,
        Title = request.Title,
        CodeBefore = request.CodeBefore,
        CodeAfter = request.CodeAfter,
        ChangeDescription = request.ChangeDescription,
        Items = request.Items.Select((item, index) => new ChecklistItem
        {
            Text = item.Text,
            IsChecked = item.IsChecked,
            SortOrder = item.SortOrder == 0 ? index : item.SortOrder
        }).ToList()
    };

    db.Checklists.Add(checklist);
    await db.SaveChangesAsync(ct);

    return Results.Created($"/api/checklists/{checklist.Id}", checklist);
});

api.MapPut("/checklists/{id:guid}", async (
    Guid id,
    ChecklistCreateRequest request,
    ApplicationDbContext db,
    ClaimsPrincipal user,
    CancellationToken ct) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var checklist = await db.Checklists
        .Include(c => c.Items)
        .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

    if (checklist is null)
        return Results.NotFound();

    checklist.Title = request.Title;
    checklist.CodeBefore = request.CodeBefore;
    checklist.CodeAfter = request.CodeAfter;
    checklist.ChangeDescription = request.ChangeDescription;
    checklist.UpdatedAt = DateTimeOffset.UtcNow;

    checklist.Items.Clear();
    foreach (var item in request.Items.Select((item, index) => new ChecklistItem
             {
                 Text = item.Text,
                 IsChecked = item.IsChecked,
                 SortOrder = item.SortOrder == 0 ? index : item.SortOrder
             }))
    {
        checklist.Items.Add(item);
    }

    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

api.MapDelete("/checklists/{id:guid}", async (Guid id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var checklist = await db.Checklists.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);
    if (checklist is null)
        return Results.NotFound();

    db.Checklists.Remove(checklist);
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

api.MapGet("/checklists/{id:guid}/export", async (
    Guid id,
    ApplicationDbContext db,
    ClaimsPrincipal user,
    CancellationToken ct) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var checklist = await db.Checklists
        .Include(c => c.Items)
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

    if (checklist is null)
        return Results.NotFound();

    var lines = new List<string>
    {
        checklist.Title,
        $"Created: {checklist.CreatedAt:O}",
        ""
    };

    foreach (var item in checklist.Items.OrderBy(i => i.SortOrder))
    {
        var status = item.IsChecked ? "[x]" : "[ ]";
        lines.Add($"{status} {item.Text}");
    }

    var content = string.Join(Environment.NewLine, lines);
    return Results.File(
        System.Text.Encoding.UTF8.GetBytes(content),
        "text/plain",
        $"checklist-{checklist.Id}.txt");
});

api.MapGet("/templates", async (ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var templates = await db.ChecklistTemplates
        .AsNoTracking()
        .Where(t => t.UserId == userId)
        .OrderByDescending(t => t.CreatedAt)
        .Select(t => new
        {
            t.Id,
            t.Name,
            t.Description,
            ItemCount = t.Items.Count
        })
        .ToListAsync(ct);

    return Results.Ok(templates);
});

api.MapPost("/templates", async (
    TemplateCreateRequest request,
    ApplicationDbContext db,
    ClaimsPrincipal user,
    CancellationToken ct) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var template = new ChecklistTemplate
    {
        UserId = userId,
        Name = request.Name,
        Description = request.Description,
        Items = request.Items.Select((item, index) => new ChecklistTemplateItem
        {
            Text = item.Text,
            SortOrder = item.SortOrder == 0 ? index : item.SortOrder
        }).ToList()
    };

    db.ChecklistTemplates.Add(template);
    await db.SaveChangesAsync(ct);

    return Results.Created($"/api/templates/{template.Id}", template);
});

api.MapPut("/templates/{id:guid}", async (
    Guid id,
    TemplateCreateRequest request,
    ApplicationDbContext db,
    ClaimsPrincipal user,
    CancellationToken ct) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var template = await db.ChecklistTemplates
        .Include(t => t.Items)
        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);

    if (template is null)
        return Results.NotFound();

    template.Name = request.Name;
    template.Description = request.Description;
    template.UpdatedAt = DateTimeOffset.UtcNow;
    template.Items.Clear();
    foreach (var item in request.Items.Select((item, index) => new ChecklistTemplateItem
             {
                 Text = item.Text,
                 SortOrder = item.SortOrder == 0 ? index : item.SortOrder
             }))
    {
        template.Items.Add(item);
    }

    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

api.MapDelete("/templates/{id:guid}", async (Guid id, ApplicationDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var template = await db.ChecklistTemplates.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);
    if (template is null)
        return Results.NotFound();

    db.ChecklistTemplates.Remove(template);
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

app.Run();

public partial class Program { }
