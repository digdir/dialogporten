using System.Text;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class EndUserContext
{
    public const string CopyCommand = "COPY \"DialogEndUserContext\" (\"Id\", \"CreatedAt\", \"UpdatedAt\", \"Revision\", \"DialogId\", \"SystemLabelId\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public static string Generate(DateTimeOffset currentDate, DateTimeOffset endDate, TimeSpan intervalSeconds)
    {
        var endUserContextCsvData = new StringBuilder();


        while (currentDate < endDate)
        {
            var dialogId = DeterministicUuidV7.Generate(currentDate, nameof(DialogEntity));
            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");

            endUserContextCsvData.AppendLine($"{dialogId},{formattedDate},{formattedDate},{Guid.NewGuid()},{dialogId},1");
            currentDate = currentDate.Add(intervalSeconds);
        }

        return endUserContextCsvData.ToString();
    }
}
