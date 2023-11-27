using Digdir.Library.Entity.Abstractions;
using System.Globalization;

namespace Digdir.Domain.Dialogporten.Domain.Localizations;

public class Localization : IJoinEntity
{
    private static readonly HashSet<string> ValidCultureNames = CultureInfo
        .GetCultures(CultureTypes.NeutralCultures | CultureTypes.SpecificCultures)
        .Where(x => !string.IsNullOrWhiteSpace(x.Name))
        .Select(x => x.Name)
        .ToHashSet();

    private string _cultureCode = null!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string Value { get; set; } = null!;
    public string CultureCode
    {
        get => _cultureCode;
        set => _cultureCode = NormalizeCultureCode(value)!;
    }

    // === Dependent relationships ===
    public Guid LocalizationSetId { get; set; }
    public LocalizationSet LocalizationSet { get; set; } = null!;

    public static string? NormalizeCultureCode(string? cultureCode) =>
        cultureCode?.Trim().Replace('_', '-').ToLowerInvariant();

    public static bool IsValidCultureCode(string? cultureCode) =>
        !string.IsNullOrWhiteSpace(cultureCode) &&
        ValidCultureNames.Contains(cultureCode, StringComparer.InvariantCultureIgnoreCase);
}
