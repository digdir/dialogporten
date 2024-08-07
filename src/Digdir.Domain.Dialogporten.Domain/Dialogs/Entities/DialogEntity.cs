﻿using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events;
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
    public string ServiceResourceType { get; set; } = null!;
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
    public List<DialogTransmission> Transmissions { get; set; } = [];

    [AggregateChild]
    public List<DialogContent> Content { get; set; } = [];

    [AggregateChild]
    public List<DialogSearchTag> SearchTags { get; set; } = [];

    [AggregateChild]
    public List<DialogAttachment> Attachments { get; set; } = [];

    [AggregateChild]
    public List<DialogGuiAction> GuiActions { get; set; } = [];

    [AggregateChild]
    public List<DialogApiAction> ApiActions { get; set; } = [];

    [AggregateChild]
    public List<DialogActivity> Activities { get; set; } = [];

    [AggregateChild]
    public List<DialogSeenLog> SeenLog { get; set; } = [];

    public void OnCreate(AggregateNode self, DateTimeOffset utcNow)
        => _domainEvents.Add(new DialogCreatedDomainEvent(Id, ServiceResource, Party));

    public void OnUpdate(AggregateNode self, DateTimeOffset utcNow)
    {
        var changedChildren = self.Children.Where(x =>
            x.State != AggregateNodeState.Unchanged &&
            x.Entity is not DialogSearchTag &&
            x.Entity is not DialogActivity);

        var shouldProduceEvent = self.IsDirectlyModified() || changedChildren.Any();
        if (shouldProduceEvent)
        {
            _domainEvents.Add(new DialogUpdatedDomainEvent(Id, ServiceResource, Party));
        }
    }

    public void OnDelete(AggregateNode self, DateTimeOffset utcNow)
        => _domainEvents.Add(new DialogDeletedDomainEvent(Id, ServiceResource, Party));

    public void UpdateSeenAt(string endUserId, DialogUserType.Values userTypeId, string? endUserName)
    {
        var lastSeenAt = SeenLog
            .Where(x => x.SeenBy.ActorId == endUserId)
            .MaxBy(x => x.CreatedAt)
            ?.CreatedAt
            ?? DateTimeOffset.MinValue;

        if (lastSeenAt >= UpdatedAt)
        {
            return;
        }

        SeenLog.Add(new()
        {
            EndUserTypeId = userTypeId,
            SeenBy = new DialogSeenLogSeenByActor
            {
                ActorTypeId = DialogActorType.Values.PartyRepresentative,
                ActorId = endUserId,
                ActorName = endUserName
            }
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
