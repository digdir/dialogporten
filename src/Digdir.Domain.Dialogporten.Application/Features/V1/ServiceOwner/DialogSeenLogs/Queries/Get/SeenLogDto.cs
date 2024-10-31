using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Actors;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogSeenLogs.Queries.Get;

public sealed class SeenLogDto
{
    public Guid Id { get; set; }
    public DateTimeOffset SeenAt { get; set; }

    public ActorDto SeenBy { get; set; } = null!;

    public bool? IsViaServiceOwner { get; set; }
}

