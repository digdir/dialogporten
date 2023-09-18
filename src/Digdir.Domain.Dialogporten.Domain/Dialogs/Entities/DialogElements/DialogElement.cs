using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;

public class DialogElement : IEntity, INotifyChange
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    public string? AuthorizationAttribute { get; set; }
    public Uri? Type { get; set; }

    // === Dependent relationships ===
    public Guid DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    public DialogElementDisplayName? DisplayName { get; set; }

    public Guid? RelatedDialogElementId { get; set; }
    public DialogElement? RelatedDialogElement { get; set; }

    // === Principal relationships ===
    public List<DialogElementUrl> Urls { get; set; } = new();
    public List<DialogApiAction> ApiActions { get; set; } = new();
    public List<DialogActivity> Activities { get; set; } = new();
    public List<DialogElement> RelatedDialogElements { get; set; } = new();
    public void Created(object? child, DateTimeOffset utcNow)
    {
        // dialog element created domain event
        // id,
        // dialogId,
        // data:
        //    something: [
        //      url/123,
        //      url/456,
        //    ]
    }

    public void Updated(object? child, DateTimeOffset utcNow)
    {
        // element updated domain event
    }

    public void Deleted(object? child, DateTimeOffset utcNow)
    {
        // dialog element deleted domain event
    }
}

public class DialogElementDisplayName : LocalizationSet
{
    public Guid ElementId { get; set; }
    public DialogElement Element { get; set; } = null!;
}