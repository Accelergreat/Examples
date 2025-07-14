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

public class PostTests : AccelergreatXunitTest
{
    public PostTests(IAccelergreatEnvironmentPool environmentPool) : base(environmentPool)
    {
    }

    [Fact]
    public async Task CanCreatePostWithBlog()
    {
        // Arrange
        var databaseComponent = GetComponent<BloggingDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Get the seeded blog
        Blog blog;
        await using (var context = dbContextFactory.NewDbContext())
        {
            blog = await context.Blogs.FirstAsync();
        }

        var newPost = new Post
        {
            Title = "Getting Started with Accelergreat",
            Content = "This is a comprehensive guide to using Accelergreat for testing.",
            CreatedAt = DateTime.UtcNow,
            BlogId = blog.Id
        };

        // Act
        await using (var context = dbContextFactory.NewDbContext())
        {
            context.Posts.Add(newPost);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = dbContextFactory.NewDbContext())
        {
            var savedPost = await context.Posts
                .Include(p => p.Blog)
                .FirstOrDefaultAsync(p => p.Title == "Getting Started with Accelergreat");
            
            savedPost.Should().NotBeNull();
            savedPost!.Blog.Should().NotBeNull();
            savedPost.Blog.Name.Should().Be("Tech Blog");
        }
    }

    [Fact]
    public async Task CanRetrievePostsForBlog()
    {
        // Arrange
        var databaseComponent = GetComponent<BloggingDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Create multiple posts for the seeded blog
        await using (var context = dbContextFactory.NewDbContext())
        {
            var blog = await context.Blogs.FirstAsync();
            
            var posts = new[]
            {
                new Post { Title = "Post 1", Content = "Content 1", CreatedAt = DateTime.UtcNow, BlogId = blog.Id },
                new Post { Title = "Post 2", Content = "Content 2", CreatedAt = DateTime.UtcNow, BlogId = blog.Id },
                new Post { Title = "Post 3", Content = "Content 3", CreatedAt = DateTime.UtcNow, BlogId = blog.Id }
            };

            context.Posts.AddRange(posts);
            await context.SaveChangesAsync();
        }

        // Act & Assert
        await using (var context = dbContextFactory.NewDbContext())
        {
            var blogWithPosts = await context.Blogs
                .Include(b => b.Posts)
                .FirstAsync();

            blogWithPosts.Posts.Should().HaveCount(3);
            blogWithPosts.Posts.Should().OnlyContain(p => p.BlogId == blogWithPosts.Id);
        }
    }

    [Fact]
    public async Task CanUpdatePost()
    {
        // Arrange
        var databaseComponent = GetComponent<BloggingDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Create a post to update
        await using (var context = dbContextFactory.NewDbContext())
        {
            var blog = await context.Blogs.FirstAsync();
            var post = new Post
            {
                Title = "Original Title",
                Content = "Original content",
                CreatedAt = DateTime.UtcNow,
                BlogId = blog.Id
            };

            context.Posts.Add(post);
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = dbContextFactory.NewDbContext())
        {
            var post = await context.Posts.FirstAsync(p => p.Title == "Original Title");
            post.Title = "Updated Title";
            post.Content = "Updated content";
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = dbContextFactory.NewDbContext())
        {
            var updatedPost = await context.Posts.FirstAsync(p => p.Title == "Updated Title");
            updatedPost.Content.Should().Be("Updated content");
        }
    }

    [Fact]
    public async Task CanDeletePost()
    {
        // Arrange
        var databaseComponent = GetComponent<BloggingDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Create a post to delete
        await using (var context = dbContextFactory.NewDbContext())
        {
            var blog = await context.Blogs.FirstAsync();
            var post = new Post
            {
                Title = "Post to Delete",
                Content = "This will be deleted",
                CreatedAt = DateTime.UtcNow,
                BlogId = blog.Id
            };

            context.Posts.Add(post);
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = dbContextFactory.NewDbContext())
        {
            var post = await context.Posts.FirstAsync(p => p.Title == "Post to Delete");
            context.Posts.Remove(post);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = dbContextFactory.NewDbContext())
        {
            var deletedPost = await context.Posts.FirstOrDefaultAsync(p => p.Title == "Post to Delete");
            deletedPost.Should().BeNull();
        }
    }

    [Fact]
    public async Task MultipleTestsRunInParallel()
    {
        // This test demonstrates that each test gets its own isolated database
        // and can run in parallel with other tests
        var databaseComponent = GetComponent<BloggingDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        var uniqueTitle = $"Parallel Test Post - {Guid.NewGuid()}";

        await using var context = dbContextFactory.NewDbContext();
        var blog = await context.Blogs.FirstAsync();
        
        var post = new Post
        {
            Title = uniqueTitle,
            Content = "This test can run in parallel with others",
            CreatedAt = DateTime.UtcNow,
            BlogId = blog.Id
        };

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var savedPost = await context.Posts.FirstOrDefaultAsync(p => p.Title == uniqueTitle);
        savedPost.Should().NotBeNull();
    }
} 