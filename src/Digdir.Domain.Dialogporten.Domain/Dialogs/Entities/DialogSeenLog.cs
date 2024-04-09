using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions.Features.Aggregate;
using Digdir.Library.Entity.Abstractions.Features.Immutable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

public class DialogSeenLog : IImmutableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public string EndUserId { get; set; } = null!;

    public string? EndUserName { get; set; }

    [AggregateChild]
    public DialogSeenLogVia? Via { get; set; }

    // === Dependent relationships ===
    public Guid DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;
}

public class DialogSeenLogVia : LocalizationSet
{
    public DialogSeenLog DialogSeenLog { get; set; } = null!;
    public Guid DialogSeenLogId { get; set; }
}
