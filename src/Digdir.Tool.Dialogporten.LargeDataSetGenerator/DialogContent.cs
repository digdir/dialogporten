using System.Text;
using Medo;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class DialogContent
{
    private const string CsvHeader = "Id,CreatedAt,UpdatedAt,MediaType,DialogId,TypeId";

    public const string CopyCommand =
        "COPY \"DialogContent\" (\"Id\", \"CreatedAt\", \"UpdatedAt\", \"MediaType\", \"DialogId\", \"TypeId\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public static (List<Guid> dialogContentIds, string dialogContentCsvData) Generate(List<Guid> dialogIds, DateTimeOffset currentDate, int intervalSeconds)
    {
        var dialogContentCsvData = new StringBuilder();
        // dialogContentCsvData.AppendLine(CsvHeader);

        List<Guid> dialogContentIds = [];
        foreach (var dialogId in dialogIds)
        {
            var contentId1 = Uuid7.NewUuid7(currentDate).ToGuid();
            var contentId2 = Uuid7.NewUuid7(currentDate).ToGuid();

            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            dialogContentCsvData.AppendLine($"{contentId1},{formattedDate},{formattedDate},'text/plain',{dialogId},1");
            dialogContentCsvData.AppendLine($"{contentId2},{formattedDate},{formattedDate},'text/plain',{dialogId},3");

            dialogContentIds.Add(contentId1);
            dialogContentIds.Add(contentId2);

            currentDate = currentDate.AddSeconds(intervalSeconds);
        }

        return (dialogContentIds, dialogContentCsvData.ToString());
    }
}
