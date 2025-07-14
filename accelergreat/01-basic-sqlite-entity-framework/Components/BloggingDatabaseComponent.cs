using System;
using System.Threading.Tasks;
using Accelergreat.EntityFramework.Sqlite;
using BasicSqliteExample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BasicSqliteExample.Components;

public class BloggingDatabaseComponent : SqliteEntityFrameworkDatabaseComponent<BloggingContext>
{
    public BloggingDatabaseComponent() : base()
    {
    }

    protected override async Task OnDatabaseInitializedAsync(BloggingContext context)
    {
        // Seed some initial data for testing
        var blog = new Blog
        {
            Name = "Tech Blog",
            Description = "A blog about technology",
            CreatedAt = DateTime.UtcNow
        };

        context.Blogs.Add(blog);
        await context.SaveChangesAsync();

        await base.OnDatabaseInitializedAsync(context);
    }
} 