using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace HttpTriggerQueueOutput;

public class Manager
{
    private readonly ILogger<Manager> _logger;

    public Manager(ILogger<Manager> log)
    {
        _logger = log;
    }

    [FunctionName("Send")]
    [OpenApiOperation(operationId: "Send", tags: new[] { "Update something" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Data), Description = "Data", Required = true)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Summary = "The success result", Description = "Operation succeeded")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "Invalid Request", Description = "Bad request")]

    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, [Queue("accessor", Connection = "StorageConnectionAppSetting")] ICollector<Data> queueCollector)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Data>(requestBody);

            if (string.IsNullOrWhiteSpace(data.Name))
                throw new System.ArgumentException("The name must have a value");

            queueCollector.Add(data);

            return new OkObjectResult("The data was queued");
        }
        catch (System.Exception ex)
        {
            _logger.LogError($"Problem: {ex.Message}");
            return new BadRequestObjectResult($"Error: {ex.Message}");
        }
    }
}
