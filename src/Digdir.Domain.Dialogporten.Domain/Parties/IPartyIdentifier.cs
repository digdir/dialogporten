using System.Diagnostics.CodeAnalysis;

namespace Digdir.Domain.Dialogporten.Domain.Parties;

public interface IPartyIdentifier
{
    static abstract string Prefix { get; }
    string Value { get; }
    static abstract bool IsValid(ReadOnlySpan<char> value);
    static abstract bool TryParse(ReadOnlySpan<char> value, [NotNullWhen(true)] out IPartyIdentifier? identifier);
    static abstract ReadOnlySpan<char> GetIdPart(ReadOnlySpan<char> value);
}
