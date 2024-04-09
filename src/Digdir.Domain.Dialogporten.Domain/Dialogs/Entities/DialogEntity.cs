using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;
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

    [AggregateChild]
    public List<DialogSeenLog> SeenLog { get; set; } = [];

    public void SoftDelete()
    {
        foreach (var dialogElement in Elements)
        {
            dialogElement.SoftDelete();
        }
    }

    public void OnCreate(AggregateNode self, DateTimeOffset utcNow)
        => _domainEvents.Add(new DialogCreatedDomainEvent(Id, ServiceResource, Party));

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
        => _domainEvents.Add(new DialogDeletedDomainEvent(Id, ServiceResource, Party));

    public void UpdateSeenAt(string endUserId, string? endUserName)
    {
        var lastSeenAt = SeenLog
            .Where(x => x.EndUserId == endUserId)
            .MaxBy(x => x.CreatedAt)
            ?.CreatedAt
            ?? DateTimeOffset.MinValue;

        if (lastSeenAt >= UpdatedAt)
        {
            return;
        }

        SeenLog.Add(new()
        {
            EndUserId = endUserId,
            EndUserName = endUserName
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
