using Accelergreat.EntityFramework.Sqlite;
using Blog.Data;
using Blog.Data.Entities;

namespace Blog.Tests.Integration;

public class BlogSqlAccelergreatComponent : SqliteEntityFrameworkDatabaseComponent<BlogDbContext>
{
    public IReadOnlyCollection<User> Users { get; private set; } = Array.Empty<User>();

    protected override async Task OnDatabaseInitializedAsync(BlogDbContext context)
    {
        // Create some users which will be available for every test execution
        var user0 = new User("Leonardo");
        var user1 = new User("Michelangelo");

        context.Set<User>().AddRange(user0, user1);

        await context.SaveChangesAsync();

        Users = new[] { user0, user1 };
    }
}