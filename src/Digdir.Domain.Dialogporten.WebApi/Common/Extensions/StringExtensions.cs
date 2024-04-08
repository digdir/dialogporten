using System.Globalization;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Extensions;

internal static class StringExtensions
{
    public static string FormatInvariant(this string pattern, params object[] args)
        => string.Format(CultureInfo.InvariantCulture, pattern, args);
}
