namespace Shared.Configurations;

public abstract class BaseBlobStorageConfiguration
{
    public string StorageAccount { get; set; }

    public string AccessKey { get; set; }
    
    public string UnFormattedUri { get; set; }
}