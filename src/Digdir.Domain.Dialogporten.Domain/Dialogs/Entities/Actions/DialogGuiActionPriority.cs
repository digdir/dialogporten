using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;

public class DialogGuiActionPriority : AbstractLookupEntity<DialogGuiActionPriority, DialogGuiActionPriority.Enum>
{
    public DialogGuiActionPriority(Enum id) : base(id) { }
    public override DialogGuiActionPriority MapValue(Enum id) => new(id);

    public enum Enum
    {
        Primary = 1,
        Secondary = 2,
        Tertiary = 3,
    }
}
