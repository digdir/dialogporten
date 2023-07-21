using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions.Features;
using Digdir.Library.Entity.Abstractions.Features.Hierarchy;
using Digdir.Library.Entity.Abstractions.Features.Immutable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

public class DialogActivity : IImmutableEntity, ISubEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Uri? ExtendedType { get; set; }
    
    public IEntityBase Parent => Dialog;

    // === Dependent relationships ===
    public DialogActivityType.Enum TypeId { get; set; }
    public DialogActivityType Type { get; set; } = null!;

    public Guid DescriptionId { get; set; }
    public LocalizationSet Description { get; set; } = null!;

    public Guid PerformedById { get; set; }
    public LocalizationSet PerformedBy { get; set; } = null!;

    public Guid DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    public Guid? RelatedActivityId { get; set; }
    public DialogActivity? RelatedActivity { get; set; }

    public Guid? DialogElementId { get; set; }
    public DialogElement? DialogElement { get; set; }

    // === Principal relationships ===
    public List<DialogActivity> RelatedActivities { get; set; } = new();
}
