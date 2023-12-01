using System.Globalization;

namespace Digdir.Domain.Dialogporten.Application.Common.Numbers;

internal static class OrganizationNumber
{
    private static readonly int[] OrgNumberWeights = { 3, 2, 7, 6, 5, 4, 3, 2 };

    public static bool IsValid(ReadOnlySpan<char> orgNumber)
    {
        return orgNumber.Length == 9
            && Mod11.TryCalculateControlDigit(orgNumber[..8], OrgNumberWeights, out var control)
            && control == int.Parse(orgNumber[8..9], CultureInfo.InvariantCulture);
    }
}
