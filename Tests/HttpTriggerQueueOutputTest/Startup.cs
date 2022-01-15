using HttpClientSetup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QueueClientSetup;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace HttpTriggerQueueOutputTest;

public class Startup
{
    public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor accessor) =>
        loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(accessor));

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<HttpTriggerService.Client>().
            AddQueueClientService("QueueClient:StorageQueueConnectionString", "QueueClient:Name").
            ConfigureHttpClient<HttpTriggerService.Client>();
    }
}