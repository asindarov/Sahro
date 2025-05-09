using Azure.Storage;
using Azure.Storage.Blobs;
using Shared.Configurations;

namespace Sahro.DocumentGen.Worker.Extensions;

public static class BlobStorageRegistrationExtensions
{
    public static IHostBuilder RegisterBlobStorage<TConfig>(this IHostBuilder builder, string configurationTypeName, string blobStorageType)
        where TConfig : BaseBlobStorageConfiguration
    {
        Console.WriteLine(nameof(TConfig));
        builder.ConfigureServices((context, services) =>
        {
            var blobStorageConfiguration = context.Configuration.GetSection(configurationTypeName).Get<TConfig>();
            var credentials =
                new StorageSharedKeyCredential(blobStorageConfiguration!.StorageAccount, blobStorageConfiguration.AccessKey);

            // format : https://{accountName}.blob.core.windows.net
            var blobUri = string.Format((string)blobStorageConfiguration.UnFormattedUri, (object?)blobStorageConfiguration.StorageAccount);

            services.AddKeyedSingleton(blobStorageType, new BlobServiceClient(
                new Uri(blobUri), credentials));
        });

        return builder;
    }
}