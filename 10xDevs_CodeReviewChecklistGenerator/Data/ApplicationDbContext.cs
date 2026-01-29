using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using _10xDevs_CodeReviewChecklistGenerator.Models;

namespace _10xDevs_CodeReviewChecklistGenerator.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Checklist> Checklists => Set<Checklist>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
    public DbSet<ChecklistTemplate> ChecklistTemplates => Set<ChecklistTemplate>();
    public DbSet<ChecklistTemplateItem> ChecklistTemplateItems => Set<ChecklistTemplateItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Checklist>(entity =>
        {
            entity.HasMany(c => c.Items)
                .WithOne(i => i.Checklist)
                .HasForeignKey(i => i.ChecklistId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ChecklistTemplate>(entity =>
        {
            entity.HasMany(t => t.Items)
                .WithOne(i => i.Template)
                .HasForeignKey(i => i.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
