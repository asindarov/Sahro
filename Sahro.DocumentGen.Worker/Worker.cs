using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Models.Enums;

namespace Sahro.DocumentGen.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly BlobServiceClient _temporaryBlobStorage;
    private readonly IServiceProvider _serviceProvider;

    private readonly ServiceBusClient _client;
    private ServiceBusReceiver _receiver = null;
    private ServiceBusSender _sender = null;
    private ServiceBusProcessor _processor = null;

    public Worker(
        ILogger<Worker> logger,
        [FromKeyedServices("temporary")] BlobServiceClient temporaryBlobStorage,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;

        var connectionString = configuration.GetConnectionString("ServiceBus");
        _client = new ServiceBusClient(connectionString);
        _temporaryBlobStorage = temporaryBlobStorage;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. receive the user input and generate the pdf 
        // 2. upload it to the temporary azure blob 
        // 3. publish message to DocumentGenerated queue with the file id in the message content

        _processor = _client.CreateProcessor("DocumentCreated");

        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;

        await _processor.StartProcessingAsync(stoppingToken);
    }
    
    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        var document = JsonSerializer.Deserialize<Shared.Models.Document>(args.Message.Body);
        if (document != null)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var stream = new MemoryStream();
                var pdfWriter = new PdfWriter(stream);
                var pdf = new PdfDocument(pdfWriter);
                var pdfDocument = new Document(pdf);

                await Task.Delay(5000);
                
                var savedDocument =
                    await dbContext.Documents.FirstOrDefaultAsync(d => d.FileId.Equals(document.FileId));
                savedDocument.Status = GenerationStatus.InProgress;
                await dbContext.SaveChangesAsync();

                var header = new Paragraph("This document has been generated by SAHRO!")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20);

                var content = new Paragraph(savedDocument.Content)
                    .SetTextAlignment(TextAlignment.JUSTIFIED)
                    .SetFontSize(12);

                pdfDocument.Add(header);
                pdfDocument.Add(content);
                pdfDocument.Close();

                var container = _temporaryBlobStorage.GetBlobContainerClient("files");

                var exists = await container.GetBlobClient(document.FileId.ToString()).ExistsAsync();

                if (!exists.Value)
                {
                    // upload generated file to temporary blob storage
                    await container.UploadBlobAsync(document.FileId.ToString(), new BinaryData(stream.ToArray()));
                }

                await Task.Delay(5000);

                savedDocument.Status = GenerationStatus.Finished;
                await dbContext.SaveChangesAsync();

                _sender = _client.CreateSender("DocumentGenerated");

                await _sender.SendMessageAsync(new ServiceBusMessage(document.FileId.ToString()));
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

}