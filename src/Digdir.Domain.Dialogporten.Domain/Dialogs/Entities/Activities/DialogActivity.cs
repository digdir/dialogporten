using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions.Features.Immutable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

public class DialogActivity : IImmutableEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Uri? ExtendedType { get; set; }

    // === Dependent relationships ===
    public DialogActivityType.Enum TypeId { get; set; }
    public DialogActivityType Type { get; set; } = null!;

    public long DescriptionId { get; set; }
    public LocalizationSet Description { get; set; } = null!;

    public long? PerformedById { get; set; }
    public LocalizationSet? PerformedBy { get; set; }

    public long DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    public long? RelatedActivityId { get; set; }
    public DialogActivity? RelatedActivity { get; set; }

    public long? DialogElementId { get; set; }
    public DialogElement? DialogElement { get; set; }

    // === Principal relationships ===
    public List<DialogActivity> RelatedActivities { get; set; } = new();
}
