using System.Text;
using Medo;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class SearchTags
{
    private const string CsvHeader = "Id,Value,CreatedAt,DialogId";

    public const string CopyCommand = "COPY \"DialogSearchTag\" (\"Id\", \"Value\", \"CreatedAt\", \"DialogId\") FROM STDIN (FORMAT csv, HEADER true, NULL '')";

    public static string Generate(List<Guid> dialogIds, DateTimeOffset currentDate, int intervalSeconds)
    {
        var searchTagCsvData = new StringBuilder();
        searchTagCsvData.AppendLine(CsvHeader);

        foreach (var dialogId in dialogIds)
        {
            var searchTagId1 = Uuid7.NewUuid7(currentDate).ToGuid();
            var searchTagId2 = Uuid7.NewUuid7(currentDate).ToGuid();
            var searchTagId3 = Uuid7.NewUuid7(currentDate).ToGuid();

            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            searchTagCsvData.AppendLine($"{searchTagId1},This is a search tag,{formattedDate},{dialogId}");
            searchTagCsvData.AppendLine($"{searchTagId2},gibberish,{formattedDate},{dialogId}");
            searchTagCsvData.AppendLine($"{searchTagId3},Should we randomize this?,{formattedDate},{dialogId}");

            currentDate = currentDate.AddSeconds(intervalSeconds);
        }

        return searchTagCsvData.ToString();
    }
}
