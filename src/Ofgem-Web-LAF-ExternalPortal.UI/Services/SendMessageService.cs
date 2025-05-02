using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Ofgem.LAF.SharedLibrary.Extensions;
using Ofgem.LAF.SharedLibrary.Models.Messaging;

namespace Ofgem_Web_LAF_ExternalPortal.Services;

public interface ISendMessageService
{
    Task SendMessage(IRunRuleMessage message);
}




public class SendMessageService : ISendMessageService
{
    //https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues?tabs=connection-string
    private readonly string? _connectionSting;
    private readonly ILogger<SendMessageService> _logger;

    public SendMessageService(IConfiguration configuration, ILogger<SendMessageService> logger)
    {
        _connectionSting = configuration["ServiceBus:ServiceBusConnection"];

        if (_connectionSting == null) throw new ArgumentException("ServiceBus:ServiceBusConnection is missing.");

        _logger = logger;
    }

    public async Task SendMessage(IRunRuleMessage message)
    {
        _logger.LogLafInformation(LogEvents.SendMessage);

        try
        {
            var messageAsString = JsonConvert.SerializeObject(message);

            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };


            var serviceBusClient =
                new ServiceBusClient(
                    _connectionSting,
                    new DefaultAzureCredential(),
                    clientOptions);

            var serviceBusSender = serviceBusClient.CreateSender("run-rules");

            // create a batch 
            using var messageBatch = await serviceBusSender.CreateMessageBatchAsync();

            for (var i = 1; i <= 1; i++)
            {
                // try adding a message to the batch
                if (!messageBatch.TryAddMessage(new ServiceBusMessage(messageAsString)))
                {
                    // if it is too large for the batch
                    throw new ArgumentException($"The message {i} is too large to fit in the batch.");
                }
            }

            try
            {
                // Use the producer client to send the batch of messages to the Service Bus queue
                await serviceBusSender.SendMessagesAsync(messageBatch);
                _logger.LogInformation("ServiceBus:ServiceBusConnection 1 message sent to run-rules");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await serviceBusSender.DisposeAsync();
                await serviceBusClient.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogLafError(LogEvents.SendMessage, ex.Message);
            throw;
        }
    }
}