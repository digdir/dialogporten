namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSeenLogs.Queries.Get;

public sealed class GetDialogSeenLogDto
{
    public Guid Id { get; set; }
    public DateTimeOffset SeenAt { get; set; }
    public GetDialogSeenLogSeenByActorDto SeenBy { get; set; } = null!;

    public bool IsViaServiceOwner { get; set; }
    public bool IsCurrentEndUser { get; set; }
}

public sealed class GetDialogSeenLogSeenByActorDto
{
    public Guid Id { get; set; }
    public string ActorName { get; set; } = null!;
    public string ActorId { get; set; } = null!;
}
