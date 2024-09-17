using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features.Creatable;
using Digdir.Library.Entity.Abstractions.Features.SoftDeletable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

public sealed class DialogLabel : IEntity, ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool Deleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public string LabelName { get; set; } = null!;
}
