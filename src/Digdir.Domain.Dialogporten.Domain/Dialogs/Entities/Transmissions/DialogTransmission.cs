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

    public DialogActor Sender { get; set; } = null!;

    public Uri? ExtendedType { get; set; }

    public DialogTransmissionType.Values TypeId { get; set; }
    public DialogTransmissionType Type { get; set; } = null!;

    // === Principal relationships ===
    public List<TransmissionContent> Content { get; set; } = [];

    public List<TransmissionAttachment> Attachments { get; set; } = [];

    // === Dependent relationships ===
    public Guid DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    public Guid? RelatedTransmissionId { get; set; }
    public DialogTransmission? RelatedTransmission { get; set; }
}
