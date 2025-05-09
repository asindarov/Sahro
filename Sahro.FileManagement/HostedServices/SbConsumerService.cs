using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;

namespace Sahro.FileManagement.HostedServices;

public class SbConsumerService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly BlobServiceClient _temporaryBlobStorage;
    private readonly BlobServiceClient _permanentBlobStorage;
    private readonly ILogger<SbConsumerService> _logger;
    private readonly ServiceBusClient _client;
    private ServiceBusProcessor _processor = null;

    public SbConsumerService(
        IConfiguration configuration, 
        [FromKeyedServices("temporary")] BlobServiceClient temporaryBlobStorage,
        [FromKeyedServices("permanent")] BlobServiceClient permanentBlobStorage,
        ILogger<SbConsumerService> logger)
    {
        _configuration = configuration;
        
        var connectionString = _configuration.GetConnectionString("ServiceBus");
        _client = new ServiceBusClient(connectionString);
        _temporaryBlobStorage = temporaryBlobStorage;
        _permanentBlobStorage = permanentBlobStorage;
        _logger = logger;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var options = new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,

            MaxConcurrentCalls = 1,
        };

        _processor = _client.CreateProcessor("DocumentGenerated", options);
        
        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;
        
        await _processor.StartProcessingAsync(cancellationToken);
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        _logger.LogDebug("Message Handler is executed!");
        var fileId = args.Message.Body.ToString();
        _logger.LogInformation(fileId);

        if (fileId != null)
        {
            var temporaryContainer = _temporaryBlobStorage.GetBlobContainerClient("files");

            var temporaryBlob = temporaryContainer.GetBlobClient(fileId);

            var temporaryBlobContent = await temporaryBlob.DownloadAsync();

            var permanentContainer = _permanentBlobStorage.GetBlobContainerClient("files");

            var exists = await permanentContainer.GetBlobClient(fileId.ToString()).ExistsAsync();

            if (!exists.Value)
            {
                // upload generated file to temporary blob storage
                await permanentContainer.UploadBlobAsync(fileId.ToString(), temporaryBlobContent.Value.Content);
            }
        }
        
        await args.CompleteMessageAsync(args.Message);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.ErrorSource);
            Console.WriteLine(args.FullyQualifiedNamespace);
            Console.WriteLine(args.EntityPath);
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }



        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
}