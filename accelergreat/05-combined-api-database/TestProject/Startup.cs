using Accelergreat.Xunit;
using CombinedExample.Components;

namespace CombinedExample;

public class Startup : IAccelergreatStartup
{
    public void Configure(IAccelergreatBuilder builder)
    {
        builder.AddAccelergreatComponent<TaskDatabaseComponent>();
        builder.AddAccelergreatComponent<TaskApiComponent>();
    }
} 