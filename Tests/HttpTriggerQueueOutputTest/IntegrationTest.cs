using ApprovalTests;
using ApprovalTests.Reporters;
using Azure.Storage.Queues;
using HttpTriggerService;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HttpTriggerQueueOutputTest;

[UseReporter(typeof(DiffReporter))]
public class IntegrationTest
{
    private readonly Client _client;
    private readonly QueueClient _queueClient;
    private readonly ILogger<IntegrationTest> _logger;

    public IntegrationTest(Client client, QueueClient queueClient, ILogger<IntegrationTest> logger)
    {
        _client = client;
        _queueClient = queueClient;
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

        _logger.LogInformation("Create the queue if not exist");
        await _queueClient.CreateIfNotExistsAsync(cancellationToken:cancellationTokenSource.Token);

        _logger.LogInformation("Deleting all old messages from the queue");
        //clear all old messages
        await _queueClient.ClearMessagesAsync(cancellationTokenSource.Token);

        _logger.LogInformation("Sending http post request");
        await _client.SendAsync(new Data { Name = "A test message", Value = 42 }, cancellationTokenSource.Token);

        _logger.LogInformation("Receiving queue message");
        var response = await _queueClient.ReceiveMessageAsync(cancellationToken: cancellationTokenSource.Token);
        var message = response.Value;
        var body = Convert.FromBase64String(message.Body.ToString());
        var text = Encoding.UTF8.GetString(body);

        _logger.LogInformation("Deleting queue message");
        await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationTokenSource.Token);

        var textForApproval = text.Split("\r\n").Where(l => !l.Contains("$AzureWebJobsParentId")).
            Aggregate(new StringBuilder(), (sb, line) => sb.Append(line), sb => sb.ToString());

        _logger.LogInformation("Verifying result:");
        _logger.LogInformation(textForApproval);
        Approvals.VerifyJson(textForApproval);

    }
}