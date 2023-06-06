using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;

public class DialogGuiActionType : AbstractLookupEntity<DialogGuiActionType, DialogGuiActionType.Enum>
{
    public DialogGuiActionType(Enum id) : base(id) { }
    public override DialogGuiActionType MapValue(Enum id) => new(id);

    public enum Enum
    {
        Primary = 1,
        Secondary = 2,
        Tertiary = 3,
    }
}
