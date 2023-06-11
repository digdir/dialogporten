using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions.Features.Immutable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

public class DialogActivity : IImmutableEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? PerformedBy { get; set; }
    public Uri? ExtendedType { get; set; }
    public LocalizationSet Description { get; set; } = null!;

    // === Dependent relationships ===

    public DialogActivityType.Enum TypeId { get; set; }
    public DialogActivityType Type { get; set; } = null!;

    public long DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;


    public long? RelatedActivityInternalId { get; set; }
    public DialogActivity? RelatedActivity { get; set; }

    public List<DialogActivity>? RelatingActivities { get; set; }

    public long? DialogElementInternalId { get; set; }
    public DialogElement? DialogElement { get; set; }
}
