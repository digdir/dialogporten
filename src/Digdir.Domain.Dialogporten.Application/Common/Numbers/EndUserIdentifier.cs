using System.Globalization;
using System.Text.RegularExpressions;

namespace Digdir.Domain.Dialogporten.Application.Common.Numbers;

internal static partial class EndUserIdentifier
{
    private static readonly int[] NorwegianIdentifierNumberWeights1 = [3, 7, 6, 1, 8, 9, 4, 5, 2, 1];
    private static readonly int[] NorwegianIdentifierNumberWeights2 = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2, 1];

    [GeneratedRegex(@"urn:altinn:([\w-]{5,20}):([\w-]{4,20})::([\w-]{11,36})", RegexOptions.None, matchTimeoutMilliseconds: 100)]
    private static partial Regex IdRegex();

    public static bool IsValid(string identifier)
    {
        var match = IdRegex().Match(identifier);

        var namespacePart = match.Groups[1].Value;
        var type = match.Groups[2].Value;
        var value = match.Groups[3].Value;

        return namespacePart switch
        {
            "person" => ValidatePerson(type, value),
            "systemuser" => ValidateSystemUser(type, value),
            _ => false
        };
    }

    private static bool ValidatePerson(string type, string value)
    {
        return type switch
        {
            "identifier-no" => ValidateNorwegianIdentifier(value),
            _ => false,
        };
    }

    private static bool ValidateSystemUser(string type, string value)
    {
        return type == "uuid" && Guid.TryParse(value, out _);
    }

    private static bool ValidateNorwegianIdentifier(ReadOnlySpan<char> norwegianIdentifier)
    {
        return norwegianIdentifier.Length == 11
               && Mod11.TryCalculateControlDigit(norwegianIdentifier[..9], NorwegianIdentifierNumberWeights1, out var control1)
               && Mod11.TryCalculateControlDigit(norwegianIdentifier[..10], NorwegianIdentifierNumberWeights2, out var control2)
               && control1 == int.Parse(norwegianIdentifier[9..10], CultureInfo.InvariantCulture)
               && control2 == int.Parse(norwegianIdentifier[10..11], CultureInfo.InvariantCulture);
    }
}
