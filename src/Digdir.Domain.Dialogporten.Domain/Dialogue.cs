using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain;

public class DialogueEntity : IEntity
{
    // TODO: Incrementing internal id for better clustered index 
    //public int InternalId { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Guid UpdatedByUserId { get; set; }
}
