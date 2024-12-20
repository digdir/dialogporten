using System.Text;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class Attachment
{
    public const string CopyCommand = "COPY \"Attachment\" (\"Id\", \"CreatedAt\", \"UpdatedAt\", \"Discriminator\", \"DialogId\", \"TransmissionId\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public static string Generate(DateTimeOffset currentDate, DateTimeOffset endDate, TimeSpan intervalSeconds)
    {
        var attachmentCsvData = new StringBuilder();

        while (currentDate < endDate)
        {
            var dialogId = DeterministicUuidV7.Generate(currentDate, nameof(DialogEntity));
            var transmissionId = DeterministicUuidV7.Generate(currentDate, nameof(DialogTransmission));

            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            attachmentCsvData.AppendLine($"{dialogId},{formattedDate},{formattedDate},'DialogAttachment',{dialogId},");
            attachmentCsvData.AppendLine($"{transmissionId},{formattedDate},{formattedDate},'DialogTransmissionAttachment',,{transmissionId}");

            currentDate = currentDate.Add(intervalSeconds);
        }

        return attachmentCsvData.ToString();
    }
}
