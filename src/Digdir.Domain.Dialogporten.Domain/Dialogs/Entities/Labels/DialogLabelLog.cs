using Digdir.Domain.Dialogporten.Domain.Actors;
using Digdir.Library.Entity.Abstractions.Features.Aggregate;
using Digdir.Library.Entity.Abstractions.Features.Immutable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Labels;

public class DialogLabelLog : IImmutableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public string Action { get; set; } = null!;

    // Magnus: IDK ka den egt jær
    // Tenker den gjør noe form for join på FK. uee det skal EF gjøre uten [AggregateChild]
    [AggregateChild]
    public Actor Actor { get; set; } = null!;


}
