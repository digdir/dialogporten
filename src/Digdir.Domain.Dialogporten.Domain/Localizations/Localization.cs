using Digdir.Library.Entity.Abstractions;
using System.Globalization;
using Digdir.Library.Entity.Abstractions.Features.Aggregate;

namespace Digdir.Domain.Dialogporten.Domain.Localizations;

public class Localization : IJoinEntity
{
    private static readonly HashSet<string> ValidCultureNames = CultureInfo
        .GetCultures(CultureTypes.NeutralCultures | CultureTypes.SpecificCultures)
        .Where(x => !string.IsNullOrWhiteSpace(x.Name))
        .Select(x => x.Name)
        .ToHashSet();

    private string cultureCode = null!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string Value { get; set; } = null!;
    public string CultureCode 
    { 
        get => cultureCode;
        set => cultureCode = NormalizeCultureCode(value)!;
    }

    // === Dependent relationships ===
    [AggregateParent]
    public LocalizationSet LocalizationSet { get; set; } = null!;
    public Guid LocalizationSetId { get; set; }

    public static string? NormalizeCultureCode(string? cultureCode) =>
        cultureCode?.Trim().Replace('_', '-').ToLower();

    public static bool IsValidCultureCode(string? cultureCode) =>
        !string.IsNullOrWhiteSpace(cultureCode) && 
        ValidCultureNames.Contains(cultureCode, StringComparer.InvariantCultureIgnoreCase);
}
