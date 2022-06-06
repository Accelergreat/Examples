using Accelergreat.Xunit;
using Accelergreat.Xunit.Attributes;
using Accelergreat.Xunit.Extensions;
using Microsoft.Extensions.DependencyInjection;

[assembly: UseAccelergreatXunitTestFramework]

namespace Blog.Tests.Integration;

public class Startup : IAccelergreatStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAccelergreatComponent<BlogSqlAccelergreatComponent>();
    }
}