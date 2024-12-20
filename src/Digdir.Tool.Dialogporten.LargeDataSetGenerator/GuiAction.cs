using System.Text;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Medo;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class GuiAction
{
    public const string CopyCommand = "COPY \"DialogGuiAction\" (\"Id\", \"CreatedAt\", \"UpdatedAt\", \"Action\", \"Url\", \"AuthorizationAttribute\", \"IsDeleteDialogAction\", \"PriorityId\", \"HttpMethodId\", \"DialogId\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public static string Generate(DateTimeOffset currentDate, DateTimeOffset endDate, TimeSpan intervalSeconds)
    {
        var guiActionCsvData = new StringBuilder();

        while (currentDate < endDate)
        {
            var dialogId = DeterministicUuidV7.Generate(currentDate, nameof(DialogEntity));

            var guiActionId1 = DeterministicUuidV7.Generate(currentDate, nameof(DialogGuiAction), 1);
            var guiActionId2 = DeterministicUuidV7.Generate(currentDate, nameof(DialogGuiAction), 2);
            var guiActionId3 = DeterministicUuidV7.Generate(currentDate, nameof(DialogGuiAction), 3);

            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");

            guiActionCsvData.AppendLine($"{guiActionId1},{formattedDate},{formattedDate},'submit','https://digdir.apps.tt02.altinn.no',NULL,FALSE,1,2,{dialogId}");
            guiActionCsvData.AppendLine($"{guiActionId2},{formattedDate},{formattedDate},'submit','https://digdir.apps.tt02.altinn.no',NULL,FALSE,2,2,{dialogId}");
            guiActionCsvData.AppendLine($"{guiActionId3},{formattedDate},{formattedDate},'submit','https://digdir.apps.tt02.altinn.no',NULL,FALSE,3,2,{dialogId}");

            currentDate = currentDate.Add(intervalSeconds);
        }

        return guiActionCsvData.ToString();
    }
}
