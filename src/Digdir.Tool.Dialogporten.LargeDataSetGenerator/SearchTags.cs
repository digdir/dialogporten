using System.Text;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;

internal static class SearchTags
{
    public const string CopyCommand = """COPY "DialogSearchTag" ("Id", "Value", "CreatedAt", "DialogId") FROM STDIN (FORMAT csv, HEADER false, NULL '')""";

    public static string Generate(DialogTimestamp dto)
    {
        var searchTagCsvData = new StringBuilder();

        var searchTagId1 = DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogSearchTag), 1);
        var searchTagId2 = DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogSearchTag), 2);
        var searchTagId3 = DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogSearchTag), 3);

        searchTagCsvData.AppendLine($"{searchTagId1},This is a search tag,{dto.FormattedTimestamp},{dto.DialogId}");
        searchTagCsvData.AppendLine($"{searchTagId2},gibberish,{dto.FormattedTimestamp},{dto.DialogId}");
        searchTagCsvData.AppendLine($"{searchTagId3},Should we randomize this?,{dto.FormattedTimestamp},{dto.DialogId}");

        return searchTagCsvData.ToString();
    }
}
