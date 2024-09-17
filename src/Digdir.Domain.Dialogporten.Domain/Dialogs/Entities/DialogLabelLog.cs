using Digdir.Domain.Dialogporten.Domain.Actors;
using Digdir.Library.Entity.Abstractions.Features.Aggregate;
using Digdir.Library.Entity.Abstractions.Features.Immutable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

public class DialogLabelLog : IImmutableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public string Action { get; set; } = null!;

    [AggregateChild]
    public Actor Actor { get; set; } = null!;


}
