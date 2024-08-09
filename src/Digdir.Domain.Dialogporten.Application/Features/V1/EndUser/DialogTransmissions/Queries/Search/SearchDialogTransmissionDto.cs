using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Actors;
using Digdir.Domain.Dialogporten.Domain.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogTransmissions.Queries.Search;

public sealed class SearchDialogTransmissionDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? AuthorizationAttribute { get; set; }
    public bool IsAuthorized { get; set; }
    public string? ExtendedType { get; set; }
    public Guid? RelatedTransmissionId { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public DialogTransmissionType.Values Type { get; set; }

    public SearchDialogTransmissionSenderActorDto Sender { get; set; } = null!;

    public SearchDialogTransmissionContentDto Content { get; set; } = null!;
    public List<SearchDialogTransmissionAttachmentDto> Attachments { get; set; } = [];
}

public sealed class SearchDialogTransmissionSenderActorDto
{
    public Guid Id { get; set; }
    public ActorType.Values ActorType { get; set; }
    public string ActorName { get; set; } = null!;
    public string ActorId { get; set; } = null!;
}

public sealed class SearchDialogTransmissionContentDto
{
    public ContentValueDto Title { get; set; } = null!;
    public ContentValueDto Summary { get; set; } = null!;
}

public sealed class SearchDialogTransmissionAttachmentDto
{
    public Guid Id { get; set; }

    public List<LocalizationDto> DisplayName { get; set; } = [];
    public List<SearchDialogTransmissionAttachmentUrlDto> Urls { get; set; } = [];
}

public sealed class SearchDialogTransmissionAttachmentUrlDto
{
    public Guid Id { get; set; }
    public Uri Url { get; set; } = null!;
    public string? MediaType { get; set; } = null!;

    public AttachmentUrlConsumerType.Values ConsumerType { get; set; }
}
