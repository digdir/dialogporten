using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features.Aggregate;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;

public sealed class TransmissionContent : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string MediaType { get; set; } = null!;

    // === Dependent relationships ===
    public Guid TransmissionId { get; set; }
    public DialogTransmission Transmission { get; set; } = null!;

    public TransmissionContentType.Values TypeId { get; set; }
    public TransmissionContentType Type { get; set; } = null!;

    // === Principal relationships ===
    [AggregateChild]
    public TransmissionContentValue Value { get; set; } = null!;
}

public sealed class TransmissionContentValue : LocalizationSet
{
    public Guid TransmissionContentId { get; set; }
    public TransmissionContent TransmissionContent { get; set; } = null!;
}
