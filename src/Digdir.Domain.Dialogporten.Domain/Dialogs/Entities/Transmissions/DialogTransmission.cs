using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;
using Digdir.Library.Entity.Abstractions.Features.Immutable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;

public class DialogTransmission : IImmutableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public string? AuthorizationAttribute { get; set; }

    public DialogTransmissionSenderActor Sender { get; set; } = null!;

    public Uri? ExtendedType { get; set; }

    // === Principal relationships ===
    public List<TransmissionContent> Content { get; set; } = [];
    public List<TransmissionAttachment> Attachments { get; set; } = [];
    public List<DialogActivity> Activities { get; set; } = [];
    public List<DialogTransmission> RelatedTransmissions { get; set; } = [];

    // === Dependent relationships ===
    public Guid DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    public Guid? RelatedTransmissionId { get; set; }
    public DialogTransmission? RelatedTransmission { get; set; }

    public DialogTransmissionType.Values TypeId { get; set; }
    public DialogTransmissionType Type { get; set; } = null!;
}
