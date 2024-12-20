using System.Text;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Medo;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class Transmission
{
    public const string CopyCommand = "COPY \"DialogTransmission\" (\"Id\", \"CreatedAt\", \"AuthorizationAttribute\", \"ExtendedType\", \"TypeId\", \"DialogId\", \"RelatedTransmissionId\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public static string Generate(DateTimeOffset currentDate, DateTimeOffset endDate, TimeSpan intervalSeconds)
    {
        var transmissionCsvData = new StringBuilder();

        List<Guid> transmissionIds = [];

        while (currentDate < endDate)
        {

            var dialogId = DeterministicUuidV7.Generate(currentDate, nameof(DialogEntity));

            var transmissionId1 = DeterministicUuidV7.Generate(currentDate, nameof(DialogTransmission), 1);
            var transmissionId2 = DeterministicUuidV7.Generate(currentDate, nameof(DialogTransmission), 2);

            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            transmissionCsvData.AppendLine($"{transmissionId1},{formattedDate},NULL,NULL,1,{dialogId},");
            transmissionCsvData.AppendLine($"{transmissionId2},{formattedDate},NULL,NULL,2,{dialogId},{transmissionId1}");

            transmissionIds.Add(transmissionId1);
            transmissionIds.Add(transmissionId2);

            currentDate = currentDate.Add(intervalSeconds);
        }

        return transmissionCsvData.ToString();
    }
}
