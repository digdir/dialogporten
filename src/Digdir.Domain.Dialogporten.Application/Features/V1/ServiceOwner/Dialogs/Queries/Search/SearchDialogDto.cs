using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Search;

public sealed class SearchDialogDto
{
    public Guid Id { get; set; }
    public string Org { get; set; } = null!;
    public string ServiceResource { get; set; } = null!;
    public string Party { get; set; } = null!;
    public string? ExtendedStatus { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? VisibleFrom { get; set; }

    public DialogStatus.Values Status { get; set; }

    public List<LocalizationDto> Title { get; set; } = new();
    public List<LocalizationDto>? SenderName { get; set; }
    public List<SearchDialogSearchTagDto> SearchTags { get; set; } = new();
}

public sealed class SearchDialogSearchTagDto
{
    public string Value { get; set; } = null!;
}
