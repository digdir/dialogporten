using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogElements.Queries.Search;

public class SearchDialogElementDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string? AuthorizationAttribute { get; set; }
    public Uri? Type { get; set; }

    // === Dependent relationships ===
    public Guid DialogId { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public List<LocalizationDto> DisplayName { get; set; } = [];

    public Guid? RelatedDialogElementId { get; set; }
}
