using Azure.Messaging.ServiceBus;
using System;

// the client that owns the connection and can be used to create senders and receivers
ServiceBusClient client;

// the sender used to publish messages to the queue
ServiceBusSender sender;

// number of messages to be sent to the queue
const int numOfMessages = 3;

var clientOptions = new ServiceBusClientOptions()
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
};
client = new ServiceBusClient("Endpoint=sb://internship-servicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=NmIJiOGIUJUv2ALxFsYVWZqYcQZyHvoO4+ASbAJ728w=", clientOptions);
sender = client.CreateSender("bus-queue");

// create a batch 
using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

//for (int i = 1; i <= numOfMessages; i++)
//{
//    // try adding a message to the batch
//    if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
//    {
//        // if it is too large for the batch
//        throw new Exception($"The message {i} is too large to fit in the batch.");
//    }
//}

if (!messageBatch.TryAddMessage(new ServiceBusMessage($@"D:\img.png")))
{
    throw new Exception($"The message is too large to fit in the batch.");
}

try
{
    // Use the producer client to send the batch of messages to the Service Bus queue
    await sender.SendMessagesAsync(messageBatch);
    Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
}
finally
{
    // Calling DisposeAsync on client types is required to ensure that network
    // resources and other unmanaged objects are properly cleaned up.
    await sender.DisposeAsync();
    await client.DisposeAsync();
}

Console.WriteLine("Press any key to end the application");
Console.ReadKey();
