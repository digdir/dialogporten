using System.Text;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

public static class SeenLog
{
    private const string CsvHeader = "Id,CreatedAt,IsViaServiceOwner,DialogId,EndUserTypeId";

    public const string CopyCommand =
        "COPY \"DialogSeenLog\" (\"Id\", \"CreatedAt\", \"IsViaServiceOwner\", \"DialogId\", \"EndUserTypeId\") FROM STDIN (FORMAT csv, HEADER true, NULL '')";

    public static (List<Guid> seenLogIds, string seenLogCsvData) Generate(List<Guid> dialogIds, DateTimeOffset currentDate, int intervalSeconds)
    {
        var seenLogCsvData = new StringBuilder();
        seenLogCsvData.AppendLine(CsvHeader);

        List<Guid> seenLogIds = [];
        foreach (var dialogId in dialogIds)
        {
            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            seenLogCsvData.AppendLine($"{dialogId},{formattedDate},FALSE,{dialogId},1");

            seenLogIds.Add(dialogId);

            currentDate = currentDate.AddSeconds(intervalSeconds);
        }

        return (seenLogIds, seenLogCsvData.ToString());
    }
}
