using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace QueueClientSetup;

public static class Setup
{
    public static IServiceCollection AddQueueClientService(this IServiceCollection services, string connectionStringKey, string queueNameKey)
    {
        if (string.IsNullOrEmpty(connectionStringKey) || string.IsNullOrEmpty(queueNameKey))
        {
            throw new ArgumentException("connectionStringKey & queueNameKey must have a valid information");
        }

        //remember to configure the "appsettings.json" to be copied (Solution properties)
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, false);

        var configuration = builder.Build();

        if (configuration == null)
            throw new ArgumentException("Can't get configuration instance");

        string connectionString = configuration[connectionStringKey];
        string queueName = configuration[queueNameKey];

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(queueName))
        {
            throw new ArgumentException($"Configuration entries {connectionStringKey} & {queueNameKey} must have a valid information");
        }

        // Instantiate a QueueClient which will be used to create and manipulate the queue
        QueueClient queueClient = new QueueClient(connectionString, queueName);
        services.AddSingleton(queueClient);

        return services;
    }
}
