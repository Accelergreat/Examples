using Accelergreat.EntityFramework.SqlServer;
using Blog.Data;
using Blog.Data.Entities;
using Microsoft.Extensions.Configuration;

namespace Blog.Tests.Integration;

public class BlogSqlAccelergreatComponent : SqlServerEntityFrameworkDatabaseComponent<BlogDbContext>
{
    public IReadOnlyCollection<User> Users { get; private set; } = Array.Empty<User>();
    public IReadOnlyCollection<Post> Posts { get; private set; } = Array.Empty<Post>();

    public BlogSqlAccelergreatComponent(IConfiguration configuration) : base(configuration)
    {
    }

    protected override async Task OnDatabaseInitializedAsync(BlogDbContext context)
    {
        // Create some users and posts which will be available for every test execution
        var user0 = new User("Leonardo");
        var user1 = new User("Michelangelo");

        context.Set<User>().AddRange(user0, user1);

        await context.SaveChangesAsync();

        Users = new[] { user0, user1 };

        var post0 = new Post(user0.UserId, "Initial Data Post", "This post will be available for every test execution");

        context.Set<Post>().AddRange(post0);

        await context.SaveChangesAsync();

        Posts = new[] { post0 };
    }

}