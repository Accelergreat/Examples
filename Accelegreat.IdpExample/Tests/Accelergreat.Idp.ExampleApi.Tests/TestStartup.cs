using Accelergreat.Idp.ExampleApi.Tests.Components;
using Accelergreat.Xunit;
using Accelergreat.Xunit.Attributes;
using Accelergreat.Xunit.Extensions;
using Microsoft.Extensions.DependencyInjection;

[assembly: UseAccelergreatXunitTestFramework]
namespace Accelergreat.Idp.ExampleApi.Tests;

public class TestStartup : IAccelergreatStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAccelergreatComponent<DatabaseComponent>();
        services.AddAccelergreatComponent<IdentityServer4Component>();
        services.AddAccelergreatComponent<ExampleApiComponent>();
    }
}