namespace Sahro.DocumentGen.Api.Responses;

public class GenerationResponse
{
    public GenerationResponse(Guid fileId)
    {
        FileId = fileId;
    }

    public Guid FileId { get; set; }
}