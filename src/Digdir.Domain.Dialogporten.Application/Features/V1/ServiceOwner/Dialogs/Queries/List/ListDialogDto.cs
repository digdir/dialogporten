using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.List;

public sealed class ListDialogDto
{
    public Guid Id { get; set; }
    public string Org { get; set; } = null!;
    public Uri ServiceResource { get; set; } = null!;
    public string Party { get; set; } = null!;
    public string? ExtendedStatus { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? VisibleFrom { get; set; }

    public DialogStatus.Enum Status { get; set; }
    
    public List<LocalizationDto> Title { get; set; } = new();
    public List<LocalizationDto>? SenderName { get; set; }
    public List<ListDialogSearchTagDto> SearchTags { get; set; } = new();
}

public sealed class ListDialogSearchTagDto
{
    public string Value { get; set; } = null!;
}
