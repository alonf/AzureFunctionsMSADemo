using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CosmosDBClientSetup;

public static class Setup
{
    public static IServiceCollection AddCosmosDBClientService(this IServiceCollection services, string endpointUrlKey, string authorizationKeyKey)
    {
        if (string.IsNullOrEmpty(endpointUrlKey) || string.IsNullOrEmpty(authorizationKeyKey))
        {
            throw new ArgumentException("endpointUrlKey & authorizationKeyKey must have a valid information");
        }

        //remember to configure the "appsettings.json" to be copied (Solution properties)
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, false);

        var configuration = builder.Build();

        if (configuration == null)
            throw new ArgumentException("Can't get configuration instance");

        string endpointUrl = configuration[endpointUrlKey];
        string authorizationKey = configuration[authorizationKeyKey];

        if (string.IsNullOrEmpty(endpointUrl) || string.IsNullOrEmpty(authorizationKey))
        {
            throw new ArgumentException($"Configuration entries {endpointUrl} & {authorizationKey} must have a valid information");
        }

        // Instantiate a QueueClient which will be used to create and manipulate the queue
        var comsosClient = new CosmosClient(endpointUrl, authorizationKey);
        services.AddSingleton(comsosClient);

        return services;
    }
}
