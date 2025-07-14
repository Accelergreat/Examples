using System;
using System.Threading.Tasks;
using Accelergreat.Environments.Pooling;
using Accelergreat.Xunit;
using BasicSqliteExample.Components;
using BasicSqliteExample.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BasicSqliteExample.Tests;

public class BlogTests : AccelergreatXunitTest
{
    public BlogTests(IAccelergreatEnvironmentPool environmentPool) : base(environmentPool)
    {
    }

    [Fact]
    public async Task CanCreateBlog()
    {
        // Arrange
        var databaseComponent = GetComponent<BloggingDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        var newBlog = new Blog
        {
            Name = "My Personal Blog",
            Description = "A blog about my thoughts and experiences",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await using (var context = dbContextFactory.NewDbContext())
        {
            context.Blogs.Add(newBlog);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = dbContextFactory.NewDbContext())
        {
            var savedBlog = await context.Blogs.FirstOrDefaultAsync(b => b.Name == "My Personal Blog");
            savedBlog.Should().NotBeNull();
            savedBlog!.Description.Should().Be("A blog about my thoughts and experiences");
        }
    }

    [Fact]
    public async Task CanRetrieveSeedData()
    {
        // Arrange
        var databaseComponent = GetComponent<BloggingDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Act & Assert
        await using var context = dbContextFactory.NewDbContext();
        var techBlog = await context.Blogs.FirstOrDefaultAsync(b => b.Name == "Tech Blog");
        
        techBlog.Should().NotBeNull();
        techBlog!.Description.Should().Be("A blog about technology");
    }

    [Fact]
    public async Task CanUpdateBlog()
    {
        // Arrange
        var databaseComponent = GetComponent<BloggingDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Act
        await using (var context = dbContextFactory.NewDbContext())
        {
            var blog = await context.Blogs.FirstAsync();
            blog.Description = "Updated description";
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = dbContextFactory.NewDbContext())
        {
            var updatedBlog = await context.Blogs.FirstAsync();
            updatedBlog.Description.Should().Be("Updated description");
        }
    }

    [Fact]
    public async Task CanDeleteBlog()
    {
        // Arrange
        var databaseComponent = GetComponent<BloggingDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Create a blog to delete
        var blogToDelete = new Blog
        {
            Name = "Temporary Blog",
            Description = "This will be deleted",
            CreatedAt = DateTime.UtcNow
        };

        await using (var context = dbContextFactory.NewDbContext())
        {
            context.Blogs.Add(blogToDelete);
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = dbContextFactory.NewDbContext())
        {
            var blog = await context.Blogs.FirstAsync(b => b.Name == "Temporary Blog");
            context.Blogs.Remove(blog);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = dbContextFactory.NewDbContext())
        {
            var deletedBlog = await context.Blogs.FirstOrDefaultAsync(b => b.Name == "Temporary Blog");
            deletedBlog.Should().BeNull();
        }
    }
} 