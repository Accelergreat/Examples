using Accelergreat.EntityFramework.PostgreSql;
using Accelergreat.Idp.Database.Contexts;
using Microsoft.Extensions.Configuration;

namespace Accelergreat.Idp.ExampleApi.Tests.Components;

internal class DatabaseComponent : PostgreSqlEntityFrameworkDatabaseComponent<BloggingContext>
{
    public DatabaseComponent(IConfiguration configuration) : base(configuration)
    {
    }

    protected override Task OnDatabaseInitializedAsync(BloggingContext context)
    {
        // Put seeding data in here
        return base.OnDatabaseInitializedAsync(context);
    }
}