using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Threading.Tasks;

ServiceBusClient busClient;

// the processor that reads and processes messages from the queue
ServiceBusProcessor processor;

var clientOptions = new ServiceBusClientOptions()
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
};
busClient = new ServiceBusClient("Endpoint=sb://internship-servicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=NmIJiOGIUJUv2ALxFsYVWZqYcQZyHvoO4+ASbAJ728w=", clientOptions);

Uri accountUri = new Uri("https://tuzstorage.blob.core.windows.net/");
BlobClient blobClient = new BlobClient(accountUri, new DefaultAzureCredential());

string connectionString = "DefaultEndpointsProtocol=https;AccountName=tuzstorage;AccountKey=vpgwCQvdSlrieBkLeUeAKAau+KjR7Y8hNNt5TJQmIUx/HEp66MMC+ZJ2KpEjEq0qRovS5n+0zez7+AStEItN+A==;EndpointSuffix=core.windows.net";
string containerName = "internship-container";
BlobContainerClient container = new BlobContainerClient(connectionString, containerName);

// create a processor that we can use to process the messages
processor = busClient.CreateProcessor("bus-queue", new ServiceBusProcessorOptions());

try
{
    // add handler to process messages
    processor.ProcessMessageAsync += MessageHandler;

    // add handler to process any errors
    processor.ProcessErrorAsync += ErrorHandler;

    // start processing 
    await processor.StartProcessingAsync();

    Console.WriteLine("Wait for a minute and then press any key to end the processing");
    Console.ReadKey();

    // stop processing 
    Console.WriteLine("\nStopping the receiver...");
    await processor.StopProcessingAsync();
    Console.WriteLine("Stopped receiving messages");
}
finally
{
    // Calling DisposeAsync on client types is required to ensure that network
    // resources and other unmanaged objects are properly cleaned up.
    await processor.DisposeAsync();
    await busClient.DisposeAsync();
}

// handle received messages
async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"Received: {body}");

    string filePath = body;
    container.UploadBlob("img.png", File.OpenRead(filePath));

    // complete the message. message is deleted from the queue. 
    await args.CompleteMessageAsync(args.Message);
}

// handle any errors when receiving messages
Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}