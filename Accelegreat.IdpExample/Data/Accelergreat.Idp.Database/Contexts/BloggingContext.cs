using Accelergreat.Idp.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Accelergreat.Idp.Database.Contexts;

public class BloggingContext : DbContext
{
    public BloggingContext(DbContextOptions<BloggingContext> options)
        : base(options)
    {
    }

    public DbSet<Blog> Blogs { get; set; } = null!;
    public DbSet<BlogEntry> BlogEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasCollation(
            name: "case_insensitive_db_level",
            locale: "en-u-ks-primary",
            provider: "icu",
            deterministic: false);
        modelBuilder.UseDefaultColumnCollation("case_insensitive_db_level");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BloggingContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}