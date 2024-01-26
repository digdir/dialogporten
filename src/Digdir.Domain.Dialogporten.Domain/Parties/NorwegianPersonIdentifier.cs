using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Digdir.Domain.Dialogporten.Domain.Numbers;

namespace Digdir.Domain.Dialogporten.Domain.Parties;

public class NorwegianPersonIdentifier : IPartyIdentifier
{
    private static readonly int[] SocialSecurityNumberWeights1 = { 3, 7, 6, 1, 8, 9, 4, 5, 2, 1 };
    private static readonly int[] SocialSecurityNumberWeights2 = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2, 1 };

    public static string Prefix { get; } = "urn:altinn:person:identifier-no::";
    public string Value { get; }

    private NorwegianPersonIdentifier(ReadOnlySpan<char> value)
    {
        Value = Prefix + value.ToString();
    }

    public static bool TryParse(ReadOnlySpan<char> value, [NotNullWhen(true)] out IPartyIdentifier? identifier)
    {
        if (!IsValid(value))
        {
            identifier = null;
            return false;
        }

        identifier = new NorwegianPersonIdentifier(GetIdPart(value));
        return true;
    }

    public static bool IsValid(ReadOnlySpan<char> value)
    {
        var idNumberWithoutPrefix = GetIdPart(value);
        return idNumberWithoutPrefix.Length == 11
               && Mod11.TryCalculateControlDigit(idNumberWithoutPrefix[..9], SocialSecurityNumberWeights1, out var control1)
               && Mod11.TryCalculateControlDigit(idNumberWithoutPrefix[..10], SocialSecurityNumberWeights2, out var control2)
               && control1 == int.Parse(idNumberWithoutPrefix[9..10], CultureInfo.InvariantCulture)
               && control2 == int.Parse(idNumberWithoutPrefix[10..11], CultureInfo.InvariantCulture);
    }

    public static ReadOnlySpan<char> GetIdPart(ReadOnlySpan<char> value)
    {
        return value.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)
            ? value[Prefix.Length..]
            : value;
    }
}
