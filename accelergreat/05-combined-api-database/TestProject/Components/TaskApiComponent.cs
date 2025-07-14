using Accelergreat.EntityFramework.Extensions;
using Accelergreat.Environments;
using Accelergreat.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using Accelergreat.EntityFramework.SqlServer.Transactions.Extensions;
using Microsoft.AspNetCore.TestHost;
using TaskApi.Data;

namespace CombinedExample.Components;

internal class TaskApiComponent : KestrelWebAppComponent<Program>
{
    protected override void BuildConfiguration(IConfigurationBuilder configurationBuilder,
        IReadOnlyAccelergreatEnvironmentPipelineData accelergreatEnvironmentPipelineData)
    {
        configurationBuilder.AddEntityFrameworkDatabaseConnectionString<TaskDbContext>(
            "DefaultConnection", accelergreatEnvironmentPipelineData);

        base.BuildConfiguration(configurationBuilder, accelergreatEnvironmentPipelineData);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder, IConfiguration configuration,
        IReadOnlyAccelergreatEnvironmentPipelineData accelergreatEnvironmentPipelineData)
    {
        builder.UseContentRoot(AppDomain.CurrentDomain.BaseDirectory);
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.AddSqlServerDbContextUsingTransaction<TaskDbContext>(
                accelergreatEnvironmentPipelineData,
                useTransactionOverriding: true);
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddSqlServerDbContextUsingTransaction<TaskDbContext>(
                accelergreatEnvironmentPipelineData,
                useTransactionOverriding: true);
        });
    }
} 