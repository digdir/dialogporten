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
        var systemUserIdWithoutPrefix = GetIdPart(value);

        if (!IsValid(systemUserIdWithoutPrefix))
        {
            identifier = null;
            return false;
        }

        identifier = new SystemUserIdentifier(systemUserIdWithoutPrefix);
        return true;
    }

    public static bool IsValid(ReadOnlySpan<char> value)
    {
        return Guid.TryParse(value, out _);
    }

    public static ReadOnlySpan<char> GetIdPart(ReadOnlySpan<char> value)
    {
        return value.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)
            ? value[Prefix.Length..]
            : value;
    }
}
