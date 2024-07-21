using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;
using Digdir.Library.Entity.Abstractions.Features.Immutable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

public class DialogSeenLog : IImmutableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public DialogActor SeenBy { get; set; } = null!;

    public bool? IsViaServiceOwner { get; set; }

    // === Dependent relationships ===
    public Guid DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    public DialogUserType.Values EndUserTypeId { get; set; }
    public DialogUserType EndUserType { get; set; } = null!;
}
