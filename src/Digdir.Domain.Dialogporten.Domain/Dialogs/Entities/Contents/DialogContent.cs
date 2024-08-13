using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features.Aggregate;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;

public sealed class DialogContent : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string MediaType { get; set; } = null!;

    // === Dependent relationships ===
    public Guid DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    public DialogContentType.Values TypeId { get; set; }
    public DialogContentType Type { get; set; } = null!;

    // === Principal relationships ===
    [AggregateChild]
    public DialogContentValue Value { get; set; } = null!;
}

public sealed class DialogContentValue : LocalizationSet
{
    public Guid DialogContentId { get; set; }
    public DialogContent DialogContent { get; set; } = null!;
}
