namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogSeenLogs.Queries.Search;

public class SearchDialogSeenLogDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public string EndUserIdHash { get; set; } = null!;

    public string? EndUserName { get; set; }
}
