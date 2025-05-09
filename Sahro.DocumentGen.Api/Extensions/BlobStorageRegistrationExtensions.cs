using Azure.Storage;
using Azure.Storage.Blobs;
using Shared.Configurations;

namespace Sahro.DocumentGen.Api.Extensions;

public static class BlobStorageRegistrationExtensions
{
    public static WebApplicationBuilder RegisterBlobStorage<TConfig>(this WebApplicationBuilder builder, string configurationTypeName, string blobStorageType)
        where TConfig : BaseBlobStorageConfiguration
    {
        var blobStorageConfiguration = builder.Configuration.GetSection(configurationTypeName).Get<TConfig>();
        var credentials =
            new StorageSharedKeyCredential(blobStorageConfiguration!.StorageAccount, blobStorageConfiguration.AccessKey);

        // format : https://{accountName}.blob.core.windows.net
        var blobUri = string.Format((string)blobStorageConfiguration.UnFormattedUri, (object?)blobStorageConfiguration.StorageAccount);

        builder.Services.AddKeyedSingleton(blobStorageType, new BlobServiceClient(
            new Uri(blobUri), credentials));

        return builder;
    }
}