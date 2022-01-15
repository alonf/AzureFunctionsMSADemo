using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;
using CosmosDBClientSetup;
using QueueClientSetup;

namespace QueueTriggerCosmosOutputTest;

public class Startup
{
    public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor accessor) =>
        loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(accessor));

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddQueueClientService("QueueClient:StorageQueueConnectionString", "QueueClient:Name")
                .AddCosmosDBClientService("CosmosDBClient:EndpointUrl", "CosmosDBClient:AuthorizationKey");

    }
}