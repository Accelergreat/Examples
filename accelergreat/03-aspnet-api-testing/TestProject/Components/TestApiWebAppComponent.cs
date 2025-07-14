using System;
using Accelergreat.Environments;
using Accelergreat.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace AspNetApiExample.Components;

internal class TestApiWebAppComponent : KestrelWebAppComponent<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder, IConfiguration configuration,
        IReadOnlyAccelergreatEnvironmentPipelineData accelergreatEnvironmentPipelineData)
    {
        builder.UseContentRoot(AppDomain.CurrentDomain.BaseDirectory);
        builder.UseEnvironment("Testing");
        
        // Configure services if needed for testing
        builder.ConfigureServices(services =>
        {
            // Additional test-specific services can be added here
        });
    }
} 