using System.Text;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

public static class EndUserContext
{
    private const string CsvHeader = "Id,CreatedAt,UpdatedAt,Revision,DialogId,SystemLabelId";
    public const string CopyCommand = "COPY \"DialogEndUserContext\" (\"Id\", \"CreatedAt\", \"UpdatedAt\", \"Revision\", \"DialogId\", \"SystemLabelId\") FROM STDIN (FORMAT csv, HEADER true, NULL '')";

    public static string Generate(List<Guid> dialogIds, DateTimeOffset currentDate, int intervalSeconds)
    {
        var endUserContextCsvData = new StringBuilder();
        endUserContextCsvData.AppendLine(CsvHeader);

        foreach (var dialogId in dialogIds)
        {
            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            endUserContextCsvData.AppendLine($"{dialogId},{formattedDate},{formattedDate},{Guid.NewGuid()},{dialogId},1");
            currentDate = currentDate.AddSeconds(intervalSeconds);
        }

        return endUserContextCsvData.ToString();
    }
}
#pragma warning restore CA1305
