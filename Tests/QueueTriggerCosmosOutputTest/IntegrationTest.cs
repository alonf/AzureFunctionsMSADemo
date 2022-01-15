using Azure.Storage.Queues;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace QueueTriggerCosmosOutputTest;

public class IntegrationTest
{
    private readonly QueueClient _queueClient;
    private readonly CosmosClient _cosmosClient;
    private readonly ILogger<IntegrationTest> _logger;

    public IntegrationTest(QueueClient queueClient, CosmosClient cosmosClient, ILogger<IntegrationTest> logger)
    {
        _queueClient = queueClient;
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

        //clear all old messages
        _logger.LogInformation("Deleting all old messages from the queue");
        await _queueClient.ClearMessagesAsync(cancellationTokenSource.Token);

        var data = new Data { Name = "Item123", Value = 42 };
        var jsonString = JsonSerializer.Serialize(data);
        _logger.LogInformation("Sending a queue message");
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
        await _queueClient.SendMessageAsync(base64, cancellationTokenSource.Token);

        _logger.LogInformation("Checking cosmos data, in a loop");

        //a default item
        Item[]? items = null;

        Policy.Handle<Exception>().OrResult(false).WaitAndRetry(10, count =>
        {
            return TimeSpan.FromSeconds(count);
        }).ExecuteAndCapture(() =>
        {
            items = collection.Container.GetItemLinqQueryable<Item>(allowSynchronousQueryExecution: true).Where(i => i.Identity == "Item123").ToArray();
            return items != null && items.Any();
        });

        Assert.NotNull(items);
        Assert.NotEmpty(items);
        Assert.Equal("Item123", items?[0].Identity);
        Assert.Equal(42 * 2, items?[0].Number);
    }
}