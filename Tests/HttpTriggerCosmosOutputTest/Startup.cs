using CosmosDBClientSetup;
using HttpClientSetup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace HttpTriggerCosmosOutputTest;

public class Startup
{
    public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor accessor) =>
        loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(accessor));

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<HttpTriggerService.Client>().ConfigureHttpClient<HttpTriggerService.Client>().
            AddCosmosDBClientService("CosmosDBClient:EndpointUrl", "CosmosDBClient:AuthorizationKey");
    }
}