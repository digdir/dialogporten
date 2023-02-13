namespace Digdir.Domain.Dialogporten.Domain.Common;

public class Localization
{
    public int LocalizationSetId { get; set; }
    public string CultureCode { get; set; } = null!;
    public string Value { get; set; } = null!;

    public LocalizationSet LocalizationSet { get; set; } = null!;
    // TODO: SKal vi ha noe culture oversikt og validering?
    //public Culture Culture { get; set; }
}

public class LocalizationSet
{
    public int Id { get; set; }

    public List<Localization> Localizations { get; set; } = new();
}
