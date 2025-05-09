using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using Sahro.FileManagement.Extensions;

namespace Sahro.FileManagement.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase
{
    private readonly BlobServiceClient _permanentBlobServiceClient;

    public FileController([FromKeyedServices("permanent")] BlobServiceClient permanentBlobServiceClient)
    {
        _permanentBlobServiceClient = permanentBlobServiceClient;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, string containerName, CancellationToken cancellationToken)
    {
        var container = _permanentBlobServiceClient.GetBlobContainerClient(containerName);
        var blob = container.GetBlobClient(file.FileName);
        
        await using var stream = file.OpenReadStream();

        await blob.UploadAsync(stream, cancellationToken);

        return NoContent();
    }

    [HttpGet("download")]
    public async Task<IActionResult> Download(string containerName, string blobName, Uri sasTokenUri, CancellationToken cancellationToken)
    {
        var sasToken = sasTokenUri.Query;
        var credentials = new AzureSasCredential(sasToken);
        var serviceClient = new BlobServiceClient(new Uri($"https://{sasTokenUri.Host}"), credentials);
        var container = serviceClient.GetBlobContainerClient(containerName);

        var blob = container.GetBlobClient(blobName);

        var blobInfo = await blob.DownloadAsync(cancellationToken);
        return File(blobInfo.Value.Content, blobName.ContentType(), Guid.NewGuid().ToString());
    }

    [HttpPost("create-sas")]
    public async Task<IActionResult> CreateSAS(string containerName, string blobName)
    {
        var container = _permanentBlobServiceClient.GetBlobContainerClient(containerName);
        var blob = container.GetBlobClient(blobName);

        var sasUri = await CreateServiceSASBlob(blob);

        if (sasUri != null)
        {
            return Ok(sasUri);
        }

        return NotFound();
    }
    
    public static Task<Uri> CreateServiceSASBlob(
        BlobClient blobClient,
        string storedPolicyName = null)
    {
        if (blobClient.CanGenerateSasUri)
        {
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                BlobName = blobClient.Name,
                Resource = "b"
            };

            if (storedPolicyName == null)
            {
                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddDays(1);
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
            }
            else
            {
                sasBuilder.Identifier = storedPolicyName;
            }

            Uri sasURI = blobClient.GenerateSasUri(sasBuilder);

            return Task.FromResult(sasURI);
        }
        else
        {
            return Task.FromResult<Uri>(null);
        }
    }
}
