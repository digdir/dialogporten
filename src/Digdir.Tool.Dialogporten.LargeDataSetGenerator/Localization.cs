using System.Text;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class Localization
{
    private const string CsvHeader = "LanguageCode,LocalizationSetId,CreatedAt,UpdatedAt,Value";
    public const string CopyCommand = "COPY \"Localization\" (\"LanguageCode\", \"LocalizationSetId\", \"CreatedAt\", \"UpdatedAt\", \"Value\") FROM STDIN (FORMAT csv, HEADER true, NULL '')";

    public static string Generate(List<Guid> localizationSetIds, DateTimeOffset currentDate)
    {
        var localizationCsvData = new StringBuilder();
        localizationCsvData.AppendLine(CsvHeader);

        foreach (var localizationSetId in localizationSetIds)
        {
            // This is wrong, the date cannot be the same for all rows.
            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            localizationCsvData.AppendLine($"nb,{localizationSetId},{formattedDate},{formattedDate},Norsk {Guid.NewGuid().ToString()[..8]}");
            localizationCsvData.AppendLine($"en,{localizationSetId},{formattedDate},{formattedDate},English {Guid.NewGuid().ToString()[..8]}");
        }

        return localizationCsvData.ToString();
    }
}
