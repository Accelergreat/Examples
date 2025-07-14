using Microsoft.EntityFrameworkCore;

namespace BasicSqliteExample.Models;

public class BloggingContext : DbContext
{
    public BloggingContext(DbContextOptions<BloggingContext> options)
        : base(options)
    {
    }

    public DbSet<Blog> Blogs { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>()
            .HasMany(b => b.Posts)
            .WithOne(p => p.Blog)
            .HasForeignKey(p => p.BlogId);

        modelBuilder.Entity<Post>()
            .Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Post>()
            .Property(p => p.Content)
            .HasMaxLength(1000);
    }
} 