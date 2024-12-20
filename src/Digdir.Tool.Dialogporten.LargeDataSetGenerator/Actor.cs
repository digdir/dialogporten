using System.Text;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class Actor
{
    private const string CsvHeader = "Id,ActorId,ActorTypeId,ActorName,Discriminator,ActivityId,DialogSeenLogId,TransmissionId,CreatedAt,UpdatedAt,LabelAssignmentLogId";

    public const string CopyCommand = "COPY \"Actor\" (\"Id\", \"ActorId\", \"ActorTypeId\", \"ActorName\", \"Discriminator\", \"ActivityId\", \"DialogSeenLogId\", \"TransmissionId\", \"CreatedAt\", \"UpdatedAt\", \"LabelAssignmentLogId\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public static string Generate(DateTimeOffset currentDate, DateTimeOffset endDate, TimeSpan interval)
    {
        var actorCsvData = new StringBuilder();

        while (currentDate < endDate)
        {
            // number of dialog activities?

            currentDate = currentDate.Add(interval);
        }

        // foreach (var dialogActivityId in dialogActivityIds)
        // {
        //     var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
        //     actorCsvData.AppendLine($"{dialogActivityId},NULL,1,'ActorName','DialogActivityPerformedByActor',{dialogActivityId},,,{formattedDate},{formattedDate},");
        // }
        //
        // foreach (var dialogSeenLogId in dialogSeenLogIds)
        // {
        //     var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
        //     actorCsvData.AppendLine($"{dialogSeenLogId},NULL,1,'ActorName','DialogSeenLogSeenByActor',,{dialogSeenLogId},,{formattedDate},{formattedDate},");
        // }
        //
        // foreach (var transmissionId in transmissionIds)
        // {
        //     var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
        //     actorCsvData.AppendLine($"{transmissionId},NULL,1,'ActorName','DialogTransmissionSenderActor',,,{transmissionId},{formattedDate},{formattedDate},");
        // }

        return actorCsvData.ToString();
    }
}
