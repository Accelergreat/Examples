using Accelergreat.Environments.Pooling;
using Accelergreat.Xunit;
using Blog.Data;
using Blog.Data.Entities;
using Blog.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Blog.Tests.Integration.Services;

public class BlogServiceTests : AccelergreatXunitTest
{
    public BlogServiceTests(IAccelergreatEnvironmentPool environmentPool) : base(environmentPool)
    {
    }

    protected BlogDbContext CreateContext()
    {
        var testSqlServerDatabaseComponent = GetComponent<BlogSqlAccelergreatComponent>();

        var dbContextFactory = testSqlServerDatabaseComponent.DbContextFactory;

        return dbContextFactory.NewDbContext();
    }

    [Fact]
    public async Task Inserts_a_Post()
    {
        var title = "A test post";
        var text = "This post was made in an integration test and was saved to a Sqlite instance managed by Accelergreat.";

        var user = GetComponent<BlogSqlAccelergreatComponent>().Users.First();

        await using var testContext = CreateContext();
        var service = new BlogService(testContext);

        var resultingPost = await service.CreatePost(user.UserId, title, text);

        // Assert the result from the service is as expected
        resultingPost.UserId.Should().Be(user.UserId);
        resultingPost.Title.Should().Be(title);
        resultingPost.Text.Should().Be(text);

        // Assert the data stored to the database is as expected
        // This is done on a separate DbContext to ensure in-memory instances are not used
        await using var assertionContext = CreateContext();
        var databasePost = await assertionContext.Set<Post>().SingleAsync(x => x.PostId == resultingPost.PostId);

        databasePost.UserId.Should().Be(user.UserId);
        databasePost.Title.Should().Be(title);
        databasePost.Text.Should().Be(text);
    }
}