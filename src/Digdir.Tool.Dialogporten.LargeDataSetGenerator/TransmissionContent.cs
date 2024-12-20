using System.Text;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions.Contents;
using Medo;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class TransmissionContent
{
    public const string CopyCommand = "COPY \"DialogTransmissionContent\" (\"Id\", \"CreatedAt\", \"UpdatedAt\", \"MediaType\", \"TransmissionId\", \"TypeId\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public static string Generate(DateTimeOffset currentDate, DateTimeOffset endDate, TimeSpan intervalSeconds)
    {
        var transmissionContentCsvData = new StringBuilder();
        // transmissionContentCsvData.AppendLine(CsvHeader);

        while (currentDate < endDate)
        {
            var transmissionId = DeterministicUuidV7.Generate(currentDate, nameof(DialogTransmission));

            var contentId1 = DeterministicUuidV7.Generate(currentDate, nameof(DialogTransmissionContent), 1);
            var contentId2 = DeterministicUuidV7.Generate(currentDate, nameof(DialogTransmissionContent), 2);

            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            transmissionContentCsvData.AppendLine($"{contentId1},{formattedDate},{formattedDate},'text/plain',{transmissionId},1");
            transmissionContentCsvData.AppendLine($"{contentId2},{formattedDate},{formattedDate},'text/plain',{transmissionId},2");

            currentDate = currentDate.Add(intervalSeconds);
        }

        return transmissionContentCsvData.ToString();
    }
}
