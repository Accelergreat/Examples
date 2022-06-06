using Microsoft.EntityFrameworkCore;

namespace Blog.Data;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}