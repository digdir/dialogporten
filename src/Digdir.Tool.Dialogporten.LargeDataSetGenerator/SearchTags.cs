using System.Text;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Medo;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class SearchTags
{
    public const string CopyCommand = "COPY \"DialogSearchTag\" (\"Id\", \"Value\", \"CreatedAt\", \"DialogId\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public static string Generate(DateTimeOffset currentDate, DateTimeOffset endDate, TimeSpan intervalSeconds)
    {
        var searchTagCsvData = new StringBuilder();

        while (currentDate < endDate)
        {
            var dialogId = DeterministicUuidV7.Generate(currentDate, nameof(DialogEntity));

            var searchTagId1 = DeterministicUuidV7.Generate(currentDate, nameof(DialogSearchTag), 1);
            var searchTagId2 = DeterministicUuidV7.Generate(currentDate, nameof(DialogSearchTag), 2);
            var searchTagId3 = DeterministicUuidV7.Generate(currentDate, nameof(DialogSearchTag), 3);

            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            searchTagCsvData.AppendLine($"{searchTagId1},This is a search tag,{formattedDate},{dialogId}");
            searchTagCsvData.AppendLine($"{searchTagId2},gibberish,{formattedDate},{dialogId}");
            searchTagCsvData.AppendLine($"{searchTagId3},Should we randomize this?,{formattedDate},{dialogId}");

            currentDate = currentDate.Add(intervalSeconds);
        }

        return searchTagCsvData.ToString();
    }
}
