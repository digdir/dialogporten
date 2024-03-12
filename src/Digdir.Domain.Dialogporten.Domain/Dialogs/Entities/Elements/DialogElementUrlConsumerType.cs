using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;

public class DialogElementUrlConsumerType : AbstractLookupEntity<DialogElementUrlConsumerType, DialogElementUrlConsumerType.Values>
{
    public DialogElementUrlConsumerType(Values id) : base(id) { }
    public override DialogElementUrlConsumerType MapValue(Values id) => new(id);

    public enum Values
    {
        Gui = 1,
        Api = 2
    }
}
