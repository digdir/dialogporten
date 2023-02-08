namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Queries.Get;

public class GetDialogueDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Guid UpdatedByUserId { get; set; }
}
