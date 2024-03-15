using System.Diagnostics;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Events.DialogElements;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features.Aggregate;
using Digdir.Library.Entity.Abstractions.Features.EventPublisher;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;

public class DialogElement : IEntity, IAggregateChangedHandler, IEventPublisher
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string? AuthorizationAttribute { get; set; }
    public Uri? Type { get; set; }

    public string? ExternalReference { get; set; }

    // === Dependent relationships ===
    public Guid DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    public Guid? RelatedDialogElementId { get; set; }
    public DialogElement? RelatedDialogElement { get; set; }

    // === Principal relationships ===
    [AggregateChild] public DialogElementDisplayName? DisplayName { get; set; }
    [AggregateChild] public List<DialogElementUrl> Urls { get; set; } = [];
    public List<DialogApiAction> ApiActions { get; set; } = [];
    public List<DialogActivity> Activities { get; set; } = [];
    public List<DialogElement> RelatedDialogElements { get; set; } = [];

    public void OnCreate(AggregateNode self, DateTimeOffset utcNow)
    {
        _domainEvents.Add(new DialogElementCreatedDomainEvent(
            DialogId, Id, Dialog.ServiceResource, Dialog.Party,
            RelatedDialogElementId?.ToString(), Type?.ToString()));
    }

    public void OnUpdate(AggregateNode self, DateTimeOffset utcNow)
    {
        _domainEvents.Add(new DialogElementUpdatedDomainEvent(
            DialogId, Id, Dialog.ServiceResource, Dialog.Party,
            RelatedDialogElementId?.ToString(), Type?.ToString()));
    }

    public void OnDelete(AggregateNode self, DateTimeOffset utcNow)
    {
        var dialog = (DialogEntity?)self.Parents
            .FirstOrDefault(x => x.Entity is DialogEntity dialog && dialog.Id == DialogId)
            ?.Entity ?? throw new UnreachableException("Expected there to be a parent dialog when deleting a dialog element.");

        _domainEvents.Add(new DialogElementDeletedDomainEvent(
            DialogId, Id, dialog.ServiceResource,
            dialog.Party, RelatedDialogElementId, Type));
    }

    public void SoftDelete()
    {
        _domainEvents.Add(new DialogElementDeletedDomainEvent(
            DialogId, Id, Dialog.ServiceResource,
            Dialog.Party, RelatedDialogElementId, Type));
    }

    private readonly List<IDomainEvent> _domainEvents = [];

    public IEnumerable<IDomainEvent> PopDomainEvents()
    {
        var events = _domainEvents.ToList();
        _domainEvents.Clear();
        return events;
    }
}

public class DialogElementDisplayName : LocalizationSet
{
    public Guid ElementId { get; set; }
    public DialogElement Element { get; set; } = null!;
}
