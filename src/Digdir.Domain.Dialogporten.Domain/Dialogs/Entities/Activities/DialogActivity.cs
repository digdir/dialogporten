﻿using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events.Activities;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions.Features.Aggregate;
using Digdir.Library.Entity.Abstractions.Features.EventPublisher;
using Digdir.Library.Entity.Abstractions.Features.Immutable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

public class DialogActivity : IImmutableEntity, IAggregateCreatedHandler, IEventPublisher
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Uri? ExtendedType { get; set; }

    // === Dependent relationships ===
    public DialogActivityType.Values TypeId { get; set; }
    public DialogActivityType Type { get; set; } = null!;

    public Guid DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    public Guid? RelatedActivityId { get; set; }
    public DialogActivity? RelatedActivity { get; set; }

    public Guid? TransmissionId { get; set; }
    public DialogTransmission? Transmission { get; set; }

    // === Principal relationships ===
    [AggregateChild]
    public DialogActivityDescription? Description { get; set; }

    [AggregateChild]
    public DialogActivityPerformedByActor PerformedBy { get; set; } = null!;

    public List<DialogActivity> RelatedActivities { get; set; } = [];

    public void OnCreate(AggregateNode self, DateTimeOffset utcNow)
    {
        _domainEvents.Add(new DialogActivityCreatedDomainEvent(
            DialogId, Id, TypeId, Dialog.Party,
            Dialog.ServiceResource, ExtendedType,
            RelatedActivityId));
    }

    private readonly List<IDomainEvent> _domainEvents = [];

    public IEnumerable<IDomainEvent> PopDomainEvents()
    {
        var events = _domainEvents.ToList();
        _domainEvents.Clear();
        return events;
    }
}

public class DialogActivityDescription : LocalizationSet
{
    public DialogActivity Activity { get; set; } = null!;
    public Guid ActivityId { get; set; }
}
