namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSeenLogs.Queries.Search;

public class SearchDialogActorDto
{
    public Guid Id { get; set; }
    public DateTimeOffset SeenAt { get; set; }

    public string ActorId { get; set; } = null!;
    public string? ActorName { get; set; }
    // todo: include ActorType? 🤔
    public bool IsCurrentEndUser { get; set; }
}
