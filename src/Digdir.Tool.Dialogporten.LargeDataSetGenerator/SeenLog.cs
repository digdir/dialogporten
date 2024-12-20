using System.Text;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class SeenLog
{
    public const string CopyCommand =
        "COPY \"DialogSeenLog\" (\"Id\", \"CreatedAt\", \"IsViaServiceOwner\", \"DialogId\", \"EndUserTypeId\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public static string Generate(DateTimeOffset currentDate, DateTimeOffset endDate, TimeSpan intervalSeconds)
    {
        var seenLogCsvData = new StringBuilder();


        while (currentDate < endDate)
        {
            var dialogId = DeterministicUuidV7.Generate(currentDate, nameof(DialogEntity));
            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            seenLogCsvData.AppendLine($"{dialogId},{formattedDate},FALSE,{dialogId},1");
            currentDate = currentDate.Add(intervalSeconds);
        }

        return seenLogCsvData.ToString();
    }
}
