namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSeenLogs.Queries.Get;

public class GetDialogActorDto
{
    public Guid Id { get; set; }
    public DateTimeOffset SeenAt { get; set; }

    public string ActorId { get; set; } = null!;
    public string? ActorName { get; set; }
    public bool IsCurrentEndUser { get; set; }
}
