using System.Globalization;
using System.Text.RegularExpressions;

namespace Digdir.Domain.Dialogporten.Application.Common.Numbers;

internal static partial class PersonIdentifier
{
    [GeneratedRegex(@"^urn:altinn:person:[\w-]+::\d+$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ValidateIdentifierRegex();

    public static bool IsValid(ReadOnlySpan<char> personIdentifier)
    {
        return ValidateIdentifierRegex().IsMatch(personIdentifier.ToString());
    }

    public static string? ExtractEndUserIdNumber(string? endUserId) =>
        endUserId?.Split("::").LastOrDefault();
}
