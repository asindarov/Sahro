using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sahro.DocumentGen.Api.Responses;
using Shared;
using Shared.Models;
using Shared.Models.Enums;

namespace Sahro.DocumentGen.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentGenerationController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusSender _serviceBusSender;
    
    public DocumentGenerationController(
        AppDbContext dbContext,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _serviceBusClient = new ServiceBusClient(configuration.GetConnectionString("ServiceBus"));
        _serviceBusSender = _serviceBusClient.CreateSender("DocumentCreated");
    }
    
    [HttpPost("generate")]
    public async Task<GenerationResponse> GeneratePdf(string input)
    {
        // 1. create document metadata 
        // 2. publish a message on DocumentCreated queue 
        // 3. Have the consumer generate the pdf and users should query GetStatus endpoint to get the file status and file id
        var fileId = Guid.NewGuid();
        var document = new Shared.Models.Document()
        {
            Id = Guid.NewGuid(),
            Content = input,
            FileId = fileId,
            Status = GenerationStatus.Started,
        };

        await _dbContext.Documents.AddAsync(document);
        await _dbContext.SaveChangesAsync();
        
        var message = new ServiceBusMessage(JsonSerializer.Serialize(document));
        await _serviceBusSender.SendMessageAsync(message);
        
        return new GenerationResponse(fileId);
    }
    
    [HttpGet("fileStatus")]
    public async Task<FileStatusResponse> GetFileGenerationStatus(Guid fileId)
    {
        var document = await _dbContext.Documents.FirstOrDefaultAsync(d => d.FileId.Equals(fileId));
        var fileStatus = string.Empty;
        
        switch (document.Status)
        {
            case GenerationStatus.Started : 
                fileStatus = "Started";
                break;
            case GenerationStatus.InProgress :
                fileStatus = "InProgress";
                break;
            case GenerationStatus.Finished:
                fileStatus = "Finished";
                break;
            case GenerationStatus.Cancelled:
                fileStatus = "Cancelled";
                break;
        };
        
        return new FileStatusResponse(document.FileId, fileStatus);
    }
}
