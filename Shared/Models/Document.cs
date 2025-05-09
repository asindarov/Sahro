using Shared.Models.Enums;

namespace Shared.Models;

public class Document
{
    public Guid Id { get; set; }

    public Guid FileId { get; set; }

    public string Content { get; set; }
    
    public GenerationStatus Status { get; set; }
}