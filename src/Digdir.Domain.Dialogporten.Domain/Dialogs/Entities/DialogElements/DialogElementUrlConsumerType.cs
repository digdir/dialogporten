using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;

public class DialogElementUrlConsumerType : AbstractLookupEntity<DialogElementUrlConsumerType, DialogElementUrlConsumerType.Enum>
{
    public DialogElementUrlConsumerType(Enum id) : base(id) { }
    public override DialogElementUrlConsumerType MapValue(Enum id) => new(id);

    public enum Enum
    {
        Gui = 1,
        Api = 2,
    }
}
