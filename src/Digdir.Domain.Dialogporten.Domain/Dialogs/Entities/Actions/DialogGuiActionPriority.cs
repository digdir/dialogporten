using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;

public class DialogGuiActionPriority : AbstractLookupEntity<DialogGuiActionPriority, DialogGuiActionPriority.Values>
{
    public DialogGuiActionPriority(Values id) : base(id) { }
    public override DialogGuiActionPriority MapValue(Values id) => new(id);

    public enum Values
    {
        Primary = 1,
        Secondary = 2,
        Tertiary = 3
    }
}
