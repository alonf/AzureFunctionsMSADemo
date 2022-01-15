using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace QueueTriggerCosmosOutput;

public class Accessor
{
    private readonly ILogger<Accessor> _logger;
    private readonly IMapper _mapper;

    public Accessor(ILogger<Accessor> logger, IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
    }

    [FunctionName("Store")]
    public async Task RunAsync([QueueTrigger("accessor", Connection = "StorageConnectionAppSetting")] Data queudItem,
        [CosmosDB(
                databaseName: "ItemDB",
                collectionName: "Items",
                ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<Item> itemsOut
        )
    {
        var item = _mapper.Map<Item>(queudItem);
        item.Number *= 2; //to show data manipulation
        await itemsOut.AddAsync(item);

        _logger.LogInformation($"C# Queue trigger function processed: {item}");
    }
}
