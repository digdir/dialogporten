using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Actions;

public class DialogueGuiActionType : AbstractLookupEntity<DialogueGuiActionType, DialogueGuiActionType.Enum>
{
    public DialogueGuiActionType(Enum id) : base(id) { }
    public override DialogueGuiActionType MapValue(Enum id) => new(id);

    public enum Enum
    {
        Primary = 1,
        Secondary = 2,
        Tertiary = 3,
    }
}
