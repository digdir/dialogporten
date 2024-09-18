using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
// Magnus: Basert på DialogStatus.cs så er det slik vi setter opp en enum?
// Amund: Krever en rename
public sealed class GlobalDisplayState(GlobalDisplayState.Values id) : AbstractLookupEntity<GlobalDisplayState, GlobalDisplayState.Values>(id)
{
    public override GlobalDisplayState MapValue(Values id) => new(id);

    public enum Values
    {
        /// <summary>
        /// Default state
        /// </summary>
        Default = 1,
        /// <summary>
        /// Used to indicate the dialog is in the "trash"
        /// </summary>
        Deleted = 2,
        /// <summary>
        /// Used to indicate the dialog is in "archive"
        /// </summary>
        Archived = 3
    }

}
