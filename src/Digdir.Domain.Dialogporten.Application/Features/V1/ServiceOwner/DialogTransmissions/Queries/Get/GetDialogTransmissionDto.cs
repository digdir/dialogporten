using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogTransmissions.Queries.Get;

public sealed class GetDialogTransmissionDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? AuthorizationAttribute { get; set; }
    public string? ExtendedType { get; set; }
    public Guid? RelatedTransmissionId { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public DialogTransmissionType.Values Type { get; set; }

    public GetDialogTransmissionSenderActorDto Sender { get; set; } = null!;

    public GetDialogTransmissionContentDto Content { get; set; } = null!;
    public List<GetDialogTransmissionAttachmentDto> Attachments { get; set; } = [];
}

public sealed class GetDialogTransmissionSenderActorDto
{
    public Guid Id { get; set; }
    public DialogActorType.Values ActorType { get; set; }
    public string ActorName { get; set; } = null!;
    public string ActorId { get; set; } = null!;
}

public sealed class GetDialogTransmissionContentDto
{
    public ContentValueDto Title { get; set; } = null!;
    public ContentValueDto Summary { get; set; } = null!;
}

public sealed class GetDialogTransmissionAttachmentDto
{
    public Guid Id { get; set; }

    public List<LocalizationDto> DisplayName { get; set; } = [];
    public List<GetDialogTransmissionAttachmentUrlDto> Urls { get; set; } = [];
}

public sealed class GetDialogTransmissionAttachmentUrlDto
{
    public Guid Id { get; set; }
    public Uri Url { get; set; } = null!;
    public string? MediaType { get; set; } = null!;

    public AttachmentUrlConsumerType.Values ConsumerType { get; set; }
}
