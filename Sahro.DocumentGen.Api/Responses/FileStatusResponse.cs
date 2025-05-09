namespace Sahro.DocumentGen.Api.Responses;

public class FileStatusResponse
{
    public FileStatusResponse(Guid fileId, string fileStatus)
    {
        FileId = fileId;
        FileStatus = fileStatus;
    }

    public Guid FileId { get; set; }
    
    public string FileStatus { get; set; }
}