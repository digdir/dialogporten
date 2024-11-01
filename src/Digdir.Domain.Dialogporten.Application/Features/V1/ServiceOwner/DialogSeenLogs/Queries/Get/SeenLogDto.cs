namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogSeenLogs.Queries.Get;

public sealed class SeenLogDto
{
    public Guid Id { get; set; }
    public DateTimeOffset SeenAt { get; set; }

    public SeenLogSeenByActorDto SeenBy { get; set; } = null!;

    public bool? IsViaServiceOwner { get; set; }
}

public sealed class SeenLogSeenByActorDto
{
    public Guid Id { get; set; }
    public string ActorName { get; set; } = null!;
    public string ActorId { get; set; } = null!;
}
