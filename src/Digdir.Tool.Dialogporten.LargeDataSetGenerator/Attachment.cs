using System.Text;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class Attachment
{
    private const string CsvHeader = "Id,CreatedAt,UpdatedAt,Discriminator,DialogId,TransmissionId";
    public const string CopyCommand = "COPY \"Attachment\" (\"Id\", \"CreatedAt\", \"UpdatedAt\", \"Discriminator\", \"DialogId\", \"TransmissionId\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public static (List<Guid> attachmentIds, string attachmentCsvData) Generate(List<Guid> dialogIds, List<Guid> transmissionIds, DateTimeOffset currentDate, int intervalSeconds)
    {
        var attachmentCsvData = new StringBuilder();
        // attachmentCsvData.AppendLine(CsvHeader);

        List<Guid> attachmentIds = [];
        foreach (var dialogId in dialogIds)
        {
            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            attachmentCsvData.AppendLine($"{dialogId},{formattedDate},{formattedDate},'DialogAttachment',{dialogId},");
            attachmentIds.Add(dialogId);

            currentDate = currentDate.AddSeconds(intervalSeconds);
        }

        foreach (var transmissionId in transmissionIds)
        {
            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            attachmentCsvData.AppendLine($"{transmissionId},{formattedDate},{formattedDate},'DialogTransmissionAttachment',,{transmissionId}");
            attachmentIds.Add(transmissionId);

            currentDate = currentDate.AddSeconds(intervalSeconds);
        }

        return (attachmentIds, attachmentCsvData.ToString());
    }
}
