using System.Globalization;
using System.Text.RegularExpressions;

namespace Digdir.Domain.Dialogporten.Application.Common.Numbers;

internal static class PersonIdentifier
{
    public static bool IsValid(ReadOnlySpan<char> personIdentifier)
    {
        var regex = new Regex(@"^urn:altinn:person:[\w-]+::\d+$");
        return regex.IsMatch(personIdentifier.ToString());
    }

    public static string? ExtractEndUserIdNumber(string? endUserId) =>
        endUserId?.Split("::").LastOrDefault();
}
