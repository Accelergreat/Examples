using Accelergreat.EntityFramework.Extensions;
using Accelergreat.Idp.Database.Contexts;
using Accelergreat.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using AccelergreatEnvironmentPipelineDataExtensions = Accelergreat.Web.Extensions.AccelergreatEnvironmentPipelineDataExtensions;

namespace Accelergreat.Idp.ExampleApi.Tests.Components;

internal class ExampleApiComponent : WebAppComponent<Program>
{
    protected override void BuildConfiguration(IConfigurationBuilder configurationBuilder,
        IReadOnlyDictionary<string, object> accelergreatEnvironmentPipelineData)
    {
        configurationBuilder.AddEntityFrameworkDatabaseConnectionString<BloggingContext>("BloggingContext",
            accelergreatEnvironmentPipelineData);
        var idpBaseUrl =AccelergreatEnvironmentPipelineDataExtensions.GetKestrelWebAppHttpBaseAddress<IdentityServer4.Startup>(
            accelergreatEnvironmentPipelineData);
        configurationBuilder.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string>("Jwt:Issuer", idpBaseUrl!)
        });
        base.BuildConfiguration(configurationBuilder, accelergreatEnvironmentPipelineData);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder, IConfiguration configuration,
        IReadOnlyDictionary<string, object> accelergreatEnvironmentPipelineData)
    {
        builder.UseContentRoot(AppDomain.CurrentDomain.BaseDirectory);
        builder.UseEnvironment("IntegrationTesting");
        base.ConfigureWebHost(builder, configuration, accelergreatEnvironmentPipelineData);
    }
}