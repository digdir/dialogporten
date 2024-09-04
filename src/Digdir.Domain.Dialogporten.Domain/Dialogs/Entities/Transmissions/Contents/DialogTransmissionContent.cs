using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features.Aggregate;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions.Contents;

public sealed class DialogTransmissionContent : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string MediaType { get; set; } = null!;

    // === Dependent relationships ===
    public Guid TransmissionId { get; set; }
    public DialogTransmission Transmission { get; set; } = null!;

    public DialogTransmissionContentType.Values TypeId { get; set; }
    public DialogTransmissionContentType Type { get; set; } = null!;

    // === Principal relationships ===
    [AggregateChild]
    public DialogTransmissionContentValue Value { get; set; } = null!;
}

public sealed class DialogTransmissionContentValue : LocalizationSet
{
    public Guid TransmissionContentId { get; set; }
    public DialogTransmissionContent TransmissionContent { get; set; } = null!;
}
