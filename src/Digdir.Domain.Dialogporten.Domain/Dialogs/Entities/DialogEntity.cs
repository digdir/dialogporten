﻿using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features.Aggregate;
using Digdir.Library.Entity.Abstractions.Features.EventPublisher;
using Digdir.Library.Entity.Abstractions.Features.SoftDeletable;
using Digdir.Library.Entity.Abstractions.Features.Versionable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

public class DialogEntity :
    IEntity,
    ISoftDeletableEntity,
    IVersionableEntity,
    IAggregateChangedHandler,
    IEventPublisher
{
    public Guid Id { get; set; }
    public Guid Revision { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool Deleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string Org { get; set; } = null!;
    public string ServiceResource { get; set; } = null!;
    public string Party { get; set; } = null!;
    public int? Progress { get; set; }
    public string? ExtendedStatus { get; set; }
    public string? ExternalReference { get; set; }
    public DateTimeOffset? VisibleFrom { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }

    // === Dependent relationships ===
    public DialogStatus.Values StatusId { get; set; }
    public DialogStatus Status { get; set; } = null!;

    // === Principal relationships ===
    [AggregateChild]
    public List<DialogContent> Content { get; set; } = [];
    [AggregateChild]
    public List<DialogSearchTag> SearchTags { get; set; } = [];

    [AggregateChild]
    public List<DialogElement> Elements { get; set; } = [];

    [AggregateChild]
    public List<DialogGuiAction> GuiActions { get; set; } = [];

    [AggregateChild]
    public List<DialogApiAction> ApiActions { get; set; } = [];

    [AggregateChild]
    public List<DialogActivity> Activities { get; set; } = [];

    public void SoftDelete()
    {
        foreach (var dialogElement in Elements)
        {
            dialogElement.SoftDelete();
        }
    }

    public void OnCreate(AggregateNode self, DateTimeOffset utcNow)
    {
        _domainEvents.Add(new DialogCreatedDomainEvent(Id, ServiceResource, Party));
    }

    public void OnUpdate(AggregateNode self, DateTimeOffset utcNow)
    {
        var changedChildren = self.Children.Where(x =>
            x.State != AggregateNodeState.Unchanged &&
            x.Entity is not DialogElement &&
            x.Entity is not DialogSearchTag &&
            x.Entity is not DialogActivity);

        var shouldProduceEvent = self.IsDirectlyModified() || changedChildren.Any();
        if (shouldProduceEvent)
        {
            _domainEvents.Add(new DialogUpdatedDomainEvent(Id, ServiceResource, Party));
        }
    }

    public void OnDelete(AggregateNode self, DateTimeOffset utcNow)
    {
        _domainEvents.Add(new DialogDeletedDomainEvent(Id, ServiceResource, Party));
    }

    public void UpdateSeenAt(string seenByEndUserId, string? seenByEndUserName)
    {
        var lastSeenByAt = Activities
            .Where(x => x.SeenByEndUserId == seenByEndUserId)
            .MaxBy(x => x.CreatedAt)
            ?.CreatedAt;

        if ((lastSeenByAt ?? DateTimeOffset.MinValue) >= UpdatedAt)
        {
            return;
        }

        var performedBy = seenByEndUserName is not null
            ? new DialogActivityPerformedBy
            {
                Localizations =
                [
                    new Localization
                    {
                        CultureCode = "nb-no",
                        Value = seenByEndUserName
                    }
                ]
            }
            : null;

        Activities.Add(new DialogActivity
        {
            PerformedBy = performedBy,
            SeenByEndUserId = seenByEndUserId,
            TypeId = DialogActivityType.Values.Seen
        });

        _domainEvents.Add(new DialogSeenDomainEvent(Id, ServiceResource, Party));
    }

    private readonly List<IDomainEvent> _domainEvents = [];
    public IEnumerable<IDomainEvent> PopDomainEvents()
    {
        var events = _domainEvents.ToList();
        _domainEvents.Clear();
        return events;
    }
}
