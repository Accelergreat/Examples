using Accelergreat.Xunit;
using BasicSqliteExample.Components;

namespace BasicSqliteExample;

public class Startup : IAccelergreatStartup
{
    public void Configure(IAccelergreatBuilder builder)
    {
        builder.AddAccelergreatComponent<BloggingDatabaseComponent>();
    }
} 