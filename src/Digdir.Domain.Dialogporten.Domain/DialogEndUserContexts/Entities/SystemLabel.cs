using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities;

public sealed class SystemLabel : AbstractLookupEntity<SystemLabel, SystemLabel.Values>
{
    public enum Values
    {
        Default = 1,
        Trash = 2,
        Archive = 3
    }
    public SystemLabel(Values id) : base(id) { }
    public override SystemLabel MapValue(Values id) => new(id);
}
