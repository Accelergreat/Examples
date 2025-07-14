using Accelergreat.Xunit;
using SqlServerExample.Components;

namespace SqlServerExample;

public class Startup : IAccelergreatStartup
{
    public void Configure(IAccelergreatBuilder builder)
    {
        builder.AddAccelergreatComponent<ECommerceDatabaseComponent>();
    }
} 