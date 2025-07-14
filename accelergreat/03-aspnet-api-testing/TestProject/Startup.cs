using Accelergreat.Xunit;
using AspNetApiExample.Components;

namespace AspNetApiExample;

public class Startup : IAccelergreatStartup
{
    public void Configure(IAccelergreatBuilder builder)
    {
        builder.AddAccelergreatComponent<TestApiWebAppComponent>();
    }
} 