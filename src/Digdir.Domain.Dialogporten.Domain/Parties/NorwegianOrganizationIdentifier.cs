using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Digdir.Domain.Dialogporten.Domain.Numbers;

namespace Digdir.Domain.Dialogporten.Domain.Parties;

public record NorwegianOrganizationIdentifier : IPartyIdentifier
{
    private static readonly int[] OrgNumberWeights = { 3, 2, 7, 6, 5, 4, 3, 2 };
    public static string Prefix { get; } = "urn:altinn:organization:identifier-no::";
    public string Value { get; }

    private NorwegianOrganizationIdentifier(ReadOnlySpan<char> value)
    {
        Value = Prefix + value.ToString();
    }

    public static bool TryParse(ReadOnlySpan<char> value, [NotNullWhen(true)] out IPartyIdentifier? identifier)
    {
        var orgNumberWithoutPrefix = GetIdPart(value);

        if (!IsValid(orgNumberWithoutPrefix))
        {
            identifier = null;
            return false;
        }

        identifier = new NorwegianOrganizationIdentifier(orgNumberWithoutPrefix);
        return true;
    }

    public static bool IsValid(ReadOnlySpan<char> value)
    {
        return value.Length == 9
               && Mod11.TryCalculateControlDigit(value[..8], OrgNumberWeights, out var control)
               && control == int.Parse(value[8..9], CultureInfo.InvariantCulture);
    }

    public static ReadOnlySpan<char> GetIdPart(ReadOnlySpan<char> value)
    {
        return value.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)
            ? value[Prefix.Length..]
            : value;
    }
}
