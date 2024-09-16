using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Actors;

public sealed class ActorType : AbstractLookupEntity<ActorType, ActorType.Values>
{
    public ActorType(Values id) : base(id) { }
    public override ActorType MapValue(Values id) => new(id);

    public enum Values
    {
        PartyRepresentative = 1,
        ServiceOwner = 2
    }
}
