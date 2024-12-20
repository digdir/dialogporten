using System.Text;
using Medo;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class Transmission
{
    private const string CsvHeader = "Id,CreatedAt,AuthorizationAttribute,ExtendedType,TypeId,DialogId,RelatedTransmissionId";

    public const string CopyCommand = "COPY \"DialogTransmission\" (\"Id\", \"CreatedAt\", \"AuthorizationAttribute\", \"ExtendedType\", \"TypeId\", \"DialogId\", \"RelatedTransmissionId\") FROM STDIN (FORMAT csv, HEADER true, NULL '')";

    public static (List<Guid> transmissionIds, string transmissionCsvData) Generate(List<Guid> dialogIds,
        DateTimeOffset currentDate, int intervalSeconds)
    {
        var transmissionCsvData = new StringBuilder();
        transmissionCsvData.AppendLine(CsvHeader);

        List<Guid> transmissionIds = [];
        foreach (var dialogId in dialogIds)
        {
            var transmissionId1 = Uuid7.NewUuid7(currentDate).ToGuid();
            var transmissionId2 = Uuid7.NewUuid7(currentDate).ToGuid();

            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            transmissionCsvData.AppendLine($"{transmissionId1},{formattedDate},NULL,NULL,1,{dialogId},");
            transmissionCsvData.AppendLine($"{transmissionId2},{formattedDate},NULL,NULL,2,{dialogId},{transmissionId1}");

            transmissionIds.Add(transmissionId1);
            transmissionIds.Add(transmissionId2);

            currentDate = currentDate.AddSeconds(intervalSeconds);
        }

        return (transmissionIds, transmissionCsvData.ToString());
    }
}
