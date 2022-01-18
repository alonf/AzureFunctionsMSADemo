using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using HttpTriggerService;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HttpTriggerQueueOutputTest;

[UseReporter(typeof(DiffReporter)), UseApprovalSubdirectory("Approvals")]
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
            await Task.Delay(TimeSpan.FromMinutes(Debugger.IsAttached ? 1000 : 1));
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
        
        Response<QueueMessage>? response = null;
        
        Policy.Handle<Exception>().OrResult(false).WaitAndRetry(10, count =>
        {
            return TimeSpan.FromSeconds(count);
        }).ExecuteAndCapture((ct) =>
        {
            ct.ThrowIfCancellationRequested();
            response = _queueClient.ReceiveMessage(cancellationToken: cancellationTokenSource.Token);
            if (response.Value == null)
            {
                _logger.LogWarning($"ReceiveMessage: Nothing.");
                return false;
            }
            //else
            _logger.LogInformation($"ReceiveMessage...");
            return true;
        }, cancellationTokenSource.Token);
        
        Assert.NotNull(response?.Value);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var message = response.Value;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        var body = Convert.FromBase64String(message.Body.ToString());
        var text = Encoding.UTF8.GetString(body);

        _logger.LogInformation($"Received text: {text}");
        _logger.LogInformation("Deleting queue message");
        await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationTokenSource.Token);

        var textForApproval = text.Split(Environment.NewLine).Where(l => !l.Contains("$AzureWebJobsParentId")).
            Aggregate(new StringBuilder(), (sb, line) => sb.Append(line), sb => sb.ToString());

        _logger.LogInformation($"Verifying result: {textForApproval}");
        _logger.LogInformation(textForApproval);
        Approvals.Verify(textForApproval);

    }
}