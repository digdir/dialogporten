using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;

public class DialogActorType : AbstractLookupEntity<DialogActorType, DialogActorType.Values>
{
    public DialogActorType(Values id) : base(id) { }
    public override DialogActorType MapValue(Values id) => new(id);

    public enum Values
    {
        PartyRepresentative = 1,
        ServiceOwner = 2
    }
}
