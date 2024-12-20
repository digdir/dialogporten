using System.Text;
using Medo;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class AttachmentUrl
{
    private const string CsvHeader = "Id,CreatedAt,MediaType,Url,ConsumerTypeId,AttachmentId";
    public const string CopyCommand = "COPY \"AttachmentUrl\" (\"Id\", \"CreatedAt\", \"MediaType\", \"Url\", \"ConsumerTypeId\", \"AttachmentId\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public static string Generate(List<Guid> attachmentIds,
        DateTimeOffset currentDate, int intervalSeconds)
    {
        var attachmentUrlCsvData = new StringBuilder();
        // attachmentUrlCsvData.AppendLine(CsvHeader);

        foreach (var attachmentId in attachmentIds)
        {
            var attachmentUrlId1 = Uuid7.NewUuid7(currentDate).ToGuid();
            var attachmentUrlId2 = Uuid7.NewUuid7(currentDate).ToGuid();

            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            attachmentUrlCsvData.AppendLine($"{attachmentUrlId1},{formattedDate},'text/plain','https://digdir.apps.tt02.altinn.no/',1,{attachmentId}");
            attachmentUrlCsvData.AppendLine($"{attachmentUrlId2},{formattedDate},'text/plain','https://digdir.apps.tt02.altinn.no/',2,{attachmentId}");

            currentDate = currentDate.AddSeconds(intervalSeconds);
        }

        return attachmentUrlCsvData.ToString();
    }
}
