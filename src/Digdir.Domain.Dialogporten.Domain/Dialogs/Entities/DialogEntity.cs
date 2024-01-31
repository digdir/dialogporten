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

    // TODO: Hent dette fra token?
    public string Org { get; set; } = "DummyOrg";
    public string ServiceResource { get; set; } = null!;
    public string Party { get; set; } = null!;
    public int? Progress { get; set; }
    public string? ExtendedStatus { get; set; }
    public string? ExternalReference { get; set; }
    public DateTimeOffset? VisibleFrom { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }

    // === Dependent relationships ===
    public DialogStatus.Values StatusId { get; set; }
    public DialogStatus Status { get; set; } = null!;

    // === Principal relationships === 
    [AggregateChild]
    public List<DialogContent> Content { get; set; } = new();
    [AggregateChild]
    public List<DialogSearchTag> SearchTags { get; set; } = new();

    [AggregateChild]
    public List<DialogElement> Elements { get; set; } = new();

    [AggregateChild]
    public List<DialogGuiAction> GuiActions { get; set; } = new();

    [AggregateChild]
    public List<DialogApiAction> ApiActions { get; set; } = new();

    [AggregateChild]
    public List<DialogActivity> Activities { get; set; } = new();

    public void SoftDelete()
    {
        foreach (var dialogElement in Elements)
        {
            dialogElement.SoftDelete();
        }
    }

    public void OnCreate(AggregateNode self, DateTimeOffset utcNow)
    {
        _domainEvents.Add(new DialogCreatedDomainEvent(Id));
    }

    public void OnUpdate(AggregateNode self, DateTimeOffset utcNow)
    {
        var shouldProduceEvent = self.IsDirectlyModified()
            || self.Children.Any(x => x
                is not AggregateNode<DialogElement>
                and not AggregateNode<DialogSearchTag>);

        if (shouldProduceEvent)
        {
            _domainEvents.Add(new DialogUpdatedDomainEvent(Id));
        }
    }

    public void OnDelete(AggregateNode self, DateTimeOffset utcNow)
    {
        _domainEvents.Add(new DialogDeletedDomainEvent(Id, ServiceResource, Party));
    }

    public void UpdateReadAt(DateTimeOffset timestamp)
    {
        if ((ReadAt ?? DateTimeOffset.MinValue) >= UpdatedAt)
        {
            return;
        }

        ReadAt = timestamp;
        _domainEvents.Add(new DialogReadDomainEvent(Id));
    }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;
}
