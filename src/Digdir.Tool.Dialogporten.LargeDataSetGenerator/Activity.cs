using System.Text;
using Medo;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class Activity
{
    private const string CsvHeader = "Id,CreatedAt,ExtendedType,TypeId,TransmissionId,DialogId";

    public const string CopyCommand = "COPY \"DialogActivity\" (\"Id\", \"CreatedAt\", \"ExtendedType\", \"TypeId\", \"TransmissionId\", \"DialogId\") FROM STDIN (FORMAT csv, HEADER true, NULL '')";

    public static (List<Guid> activityIds, string activityCsvData) Generate(List<Guid> dialogIds, DateTimeOffset currentDate, int intervalSeconds)
    {
        var activityCsvData = new StringBuilder();
        activityCsvData.AppendLine(CsvHeader);

        List<Guid> activityIds = [];
        foreach (var dialogId in dialogIds)
        {
            var activityId1 = Uuid7.NewUuid7(currentDate).ToGuid();
            var activityId2 = Uuid7.NewUuid7(currentDate).ToGuid();

            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            activityCsvData.AppendLine($"{activityId1},{formattedDate},NULL,1,,{dialogId}");
            activityCsvData.AppendLine($"{activityId2},{formattedDate},NULL,3,,{dialogId}");

            // activityIds.Add(activityId1);
            activityIds.Add(activityId2); // Only add information activities, DialogCreated activities do not have LocalizationSets

            currentDate = currentDate.AddSeconds(intervalSeconds);
        }

        return (activityIds, activityCsvData.ToString());
    }
}
