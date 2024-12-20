using System.Text;
using Medo;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class TransmissionContent
{
    private const string CsvHeader = "Id,CreatedAt,UpdatedAt,MediaType,TransmissionId,TypeId";

    public const string CopyCommand = "COPY \"DialogTransmissionContent\" (\"Id\", \"CreatedAt\", \"UpdatedAt\", \"MediaType\", \"TransmissionId\", \"TypeId\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public static (List<Guid> transmissionContentIds, string transmissionContentCsvData) Generate(List<Guid> transmissionIds, DateTimeOffset currentDate, int intervalSeconds)
    {
        var transmissionContentCsvData = new StringBuilder();
        // transmissionContentCsvData.AppendLine(CsvHeader);

        List<Guid> transmissionContentIds = [];
        foreach (var transmissionId in transmissionIds)
        {
            var contentId1 = Uuid7.NewUuid7(currentDate).ToGuid();
            var contentId2 = Uuid7.NewUuid7(currentDate).ToGuid();

            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            transmissionContentCsvData.AppendLine($"{contentId1},{formattedDate},{formattedDate},'text/plain',{transmissionId},1");
            transmissionContentCsvData.AppendLine($"{contentId2},{formattedDate},{formattedDate},'text/plain',{transmissionId},2");

            transmissionContentIds.Add(contentId1);
            transmissionContentIds.Add(contentId2);

            currentDate = currentDate.AddSeconds(intervalSeconds);
        }

        return (transmissionContentIds, transmissionContentCsvData.ToString());
    }
}
