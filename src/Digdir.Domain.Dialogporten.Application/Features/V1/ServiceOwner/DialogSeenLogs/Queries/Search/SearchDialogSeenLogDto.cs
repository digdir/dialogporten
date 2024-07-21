namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogSeenLogs.Queries.Search;

public sealed class SearchDialogSeenLogDto
{
    public Guid Id { get; set; }
    public DateTimeOffset SeenAt { get; set; }

    public SearchDialogSeenLogSeenByDto SeenBy { get; set; } = null!;

    public bool? IsViaServiceOwner { get; set; }
}

public sealed class SearchDialogSeenLogSeenByDto
{
    public string ActorName { get; set; } = null!;
    public string ActorId { get; set; } = null!;
}
