using System.Globalization;

namespace Digdir.Domain.Dialogporten.Application.Common.Numbers;

internal static class SocialSecurityNumber
{
    private static readonly int[] SocialSecurityNumberWeights1 = { 3, 7, 6, 1, 8, 9, 4, 5, 2, 1 };
    private static readonly int[] SocialSecurityNumberWeights2 = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2, 1 };

    public static bool IsValid(ReadOnlySpan<char> socialSecurityNumber)
    {
        return socialSecurityNumber.Length == 11
            && Mod11.TryCalculateControlDigit(socialSecurityNumber[..9], SocialSecurityNumberWeights1, out var control1)
            && Mod11.TryCalculateControlDigit(socialSecurityNumber[..10], SocialSecurityNumberWeights2, out var control2)
            && control1 == int.Parse(socialSecurityNumber[9..10], CultureInfo.InvariantCulture)
            && control2 == int.Parse(socialSecurityNumber[10..11], CultureInfo.InvariantCulture);
    }
}
