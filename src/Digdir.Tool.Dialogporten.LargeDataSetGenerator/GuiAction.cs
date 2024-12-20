using System.Text;
using Medo;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class GuiAction
{
    private const string CsvHeader = "Id,CreatedAt,UpdatedAt,Action,Url,AuthorizationAttribute,IsDeleteDialogAction,PriorityId,HttpMethodId,DialogId";

    public const string CopyCommand = "COPY \"DialogGuiAction\" (\"Id\", \"CreatedAt\", \"UpdatedAt\", \"Action\", \"Url\", \"AuthorizationAttribute\", \"IsDeleteDialogAction\", \"PriorityId\", \"HttpMethodId\", \"DialogId\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public static (List<Guid> guiActionIds, string guiActionCsvData) Generate(List<Guid> dialogIds, DateTimeOffset currentDate, int intervalSeconds)
    {
        var guiActionCsvData = new StringBuilder();
        // guiActionCsvData.AppendLine(CsvHeader);

        List<Guid> guiActionIds = [];
        foreach (var dialogId in dialogIds)
        {
            var guiActionId1 = Uuid7.NewUuid7(currentDate).ToGuid();
            var guiActionId2 = Uuid7.NewUuid7(currentDate).ToGuid();
            var guiActionId3 = Uuid7.NewUuid7(currentDate).ToGuid();

            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            guiActionCsvData.AppendLine($"{guiActionId1},{formattedDate},{formattedDate},'submit','https://digdir.apps.tt02.altinn.no',NULL,FALSE,1,2,{dialogId}");
            guiActionCsvData.AppendLine($"{guiActionId2},{formattedDate},{formattedDate},'submit','https://digdir.apps.tt02.altinn.no',NULL,FALSE,2,2,{dialogId}");
            guiActionCsvData.AppendLine($"{guiActionId3},{formattedDate},{formattedDate},'submit','https://digdir.apps.tt02.altinn.no',NULL,FALSE,3,2,{dialogId}");

            guiActionIds.Add(guiActionId1);
            guiActionIds.Add(guiActionId2);
            guiActionIds.Add(guiActionId3);

            currentDate = currentDate.AddSeconds(intervalSeconds);
        }

        return (guiActionIds, guiActionCsvData.ToString());
    }
}
