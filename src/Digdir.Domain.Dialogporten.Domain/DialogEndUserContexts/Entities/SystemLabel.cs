using System.Diagnostics;
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

public static class LabelExtensions
{
    private static string Namespace { get; } = "dp:systemlabel:";
    public static string ToNamespacedName(this SystemLabel.Values label) => label switch
    {

        SystemLabel.Values.Default => "Default",
        SystemLabel.Values.Trash => Namespace + "Trash",
        SystemLabel.Values.Archive => Namespace + "Archive",
        _ => throw new ArgumentOutOfRangeException(nameof(label), label, null)
    };
}
