using System.Diagnostics.CodeAnalysis;

namespace Digdir.Domain.Dialogporten.Domain.Parties;

public record SystemUserIdentifier : IPartyIdentifier
{
    public static string Prefix { get; } = "urn:altinn:systemuser::";
    public string Value { get; }

    private SystemUserIdentifier(ReadOnlySpan<char> value)
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

        identifier = new SystemUserIdentifier(GetIdPart(value));
        return true;
    }

    public static bool IsValid(ReadOnlySpan<char> value)
    {
        var idNumberWithoutPrefix = GetIdPart(value);
        return Guid.TryParse(idNumberWithoutPrefix, out _);
    }

    public static ReadOnlySpan<char> GetIdPart(ReadOnlySpan<char> value)
    {
        return value.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)
            ? value[Prefix.Length..]
            : value;
    }
}
