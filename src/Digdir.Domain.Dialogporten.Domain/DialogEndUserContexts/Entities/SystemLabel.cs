using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities;

public sealed class SystemLabel : AbstractLookupEntity<SystemLabel, SystemLabel.Values>
{
    public enum Values
    {
        Default = 1,
        Bin = 2,
        Archive = 3
    }

    public SystemLabel(Values id) : base(id) { }
    public override SystemLabel MapValue(Values id) => new(id);
}

public static class SystemLabelExtensions
{
    private static string Namespace { get; } = "systemlabel:";
    public static string ToNamespacedName(this SystemLabel.Values label) => label switch
    {

        SystemLabel.Values.Default => label.ToString(),
        SystemLabel.Values.Bin => Namespace + label,
        SystemLabel.Values.Archive => Namespace + label,
        _ => throw new ArgumentOutOfRangeException(nameof(label), label, null)
    };
}
