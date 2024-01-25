using System.Globalization;
using System.Text.RegularExpressions;

namespace Digdir.Domain.Dialogporten.Application.Common.Numbers;

public static partial class OrganizationIdentifier
{
    public const string NorwegianOrganizationIdentifierPrefix = "urn:altinn:organization:identifier-no::";
    private static readonly int[] NorwegianOrgNumberWeights = [3, 2, 7, 6, 5, 4, 3, 2];

    [GeneratedRegex(@"urn:altinn:([\w-]{5,20}):([\w-]{4,20})::([\w-]{5,36})", RegexOptions.None, matchTimeoutMilliseconds: 100)]
    private static partial Regex IdRegex();

    public static bool IsValid(string identifier)
    {
        var match = IdRegex().Match(identifier);

        var namespacePart = match.Groups[1].Value;
        var type = match.Groups[2].Value;
        var value = match.Groups[3].Value;

        return namespacePart switch
        {
            "organization" => ValidateOrganization(type, value),
            _ => false
        };
    }

    private static bool ValidateOrganization(string type, string value)
    {
        return type switch
        {
            "identifier-no" => ValidateNorwegianOrganizationIdentifier(value),
            _ => false,
        };
    }

    internal static bool ValidateNorwegianOrganizationIdentifier(ReadOnlySpan<char> norwegianOrgIdentifier)
    {
        return norwegianOrgIdentifier.Length == 9
               && Mod11.TryCalculateControlDigit(norwegianOrgIdentifier[..8], NorwegianOrgNumberWeights, out var control)
               && control == int.Parse(norwegianOrgIdentifier[8..9], CultureInfo.InvariantCulture);
    }
}
