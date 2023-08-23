using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogElements.Queries.Get;

public sealed class GetDialogElementDto
{
    public Guid Id { get; set; }
    public Uri? Type { get; set; }
    public string? AuthorizationAttribute { get; set; }

    public Guid? RelatedDialogElementId { get; set; }

    public List<LocalizationDto> DisplayName { get; set; } = new();
    public List<GetDialogElementUrlDto> Urls { get; set; } = new();
}

public sealed class GetDialogElementUrlDto
{
    public Guid Id { get; set; }
    public Uri Url { get; set; } = null!;
    public string? MimeType { get; set; }

    public DialogElementUrlConsumerType.Enum ConsumerType { get; set; }
}