using HttpTriggerService;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HttpTriggerCosmosOutputTest;

public class IntegrationTest
{
    private readonly Client _client;
    private readonly CosmosClient _cosmosClient;
    private readonly ILogger<IntegrationTest> _logger;

    public IntegrationTest(Client client, CosmosClient cosmosClient, ILogger<IntegrationTest> logger)
    {
        _client = client;
        _cosmosClient = cosmosClient;
        _logger = logger;
    }

    [Fact]
    public async Task TestSendingMessageAsync()
    {
        //a minute limit for the entire test
        var cancellationTokenSource = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMinutes(1));
            cancellationTokenSource.Cancel();
        });

        var dataBaseName = "ItemDB";
        var containerName = "Items";

        //Delete the database to make sure the test start fresh
        _logger.LogInformation("Deleting old cosmos db if exist");
        var database = _cosmosClient.GetDatabase(dataBaseName);
        try
        {
            DatabaseResponse databaseResourceResponse = await database.DeleteAsync(cancellationToken: cancellationTokenSource.Token);
        }
        catch (CosmosException ex)
        {
            _logger.LogInformation($"Ignore if the DB is not exist, or there is other problem. Error:{ex.Message}");
        }

        //Creating Cosmos DB and container
        _logger.LogInformation("Creating Cosmos DB database");
        var dataBase = await _cosmosClient.CreateDatabaseIfNotExistsAsync("ItemDB", cancellationToken: cancellationTokenSource.Token);

        //Create Items container
        _logger.LogInformation("Creating Cosmos DB item container");
        var collection = await _cosmosClient.GetDatabase("ItemDB").CreateContainerIfNotExistsAsync(containerName, "/Identity", cancellationToken: cancellationTokenSource.Token);

        _logger.LogInformation("Sending http post request");
        await _client.SendAsync(new Data { Name = "Item42", Value = 42 }, cancellationTokenSource.Token);

        //a default item
        Item[]? items = null;

        Policy.Handle<Exception>().OrResult(false).WaitAndRetry(10, count =>
        {
            return TimeSpan.FromSeconds(count);
        }).ExecuteAndCapture(() =>
        {
            items = collection.Container.GetItemLinqQueryable<Item>(allowSynchronousQueryExecution: true).Where(i => i.Identity == "Item42").ToArray();
            return items != null && items.Any();
        });

        Assert.NotNull(items);
        Assert.NotEmpty(items);
        Assert.Equal("Item42", items?[0].Identity);
        Assert.Equal(42 * 2, items?[0].Number);


    }
}