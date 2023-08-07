using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions.Features;
using Digdir.Library.Entity.Abstractions.Features.Immutable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

public class DialogActivity : IImmutableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Uri? ExtendedType { get; set; }

    // === Dependent relationships ===
    public DialogActivityType.Enum TypeId { get; set; }
    public DialogActivityType Type { get; set; } = null!;

    public DialogActivityDescription? Description { get; set; }

    public DialogActivityPerformedBy? PerformedBy { get; set; }

    public Guid DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    public Guid? RelatedActivityId { get; set; }
    public DialogActivity? RelatedActivity { get; set; }

    public Guid? DialogElementId { get; set; }
    public DialogElement? DialogElement { get; set; }

    // === Principal relationships ===
    public List<DialogActivity> RelatedActivities { get; set; } = new();
}

public class DialogActivityDescription : LocalizationSet
{
    public Guid ActivityId { get; set; }
    public DialogActivity Activity { get; set; } = null!;
}

public class DialogActivityPerformedBy : LocalizationSet
{
    public Guid ActivityId { get; set; }
    public DialogActivity Activity { get; set; } = null!;
}