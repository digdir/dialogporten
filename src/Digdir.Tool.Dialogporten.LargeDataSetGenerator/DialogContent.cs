using System.Text;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class DialogContent
{
    public const string CopyCommand =
        "COPY \"DialogContent\" (\"Id\", \"CreatedAt\", \"UpdatedAt\", \"MediaType\", \"DialogId\", \"TypeId\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public const int EntitiesPerParent = 2;

    public static string Generate(DialogTimestamp dto)
    {
        var contentId1 = DeterministicUuidV7.Generate(dto.Timestamp, nameof(Domain.Dialogporten.Domain.Dialogs.Entities.Contents.DialogContent), 1);
        var contentId2 = DeterministicUuidV7.Generate(dto.Timestamp, nameof(Domain.Dialogporten.Domain.Dialogs.Entities.Contents.DialogContent), 2);

        var dialogContentCsvData = new StringBuilder();

        var formattedDate = dto.Timestamp.ToString("yyyy-MM-dd HH:mm:ss zzz");
        dialogContentCsvData.AppendLine($"{contentId1},{formattedDate},{formattedDate},'text/plain',{dto.DialogId},1");
        dialogContentCsvData.AppendLine($"{contentId2},{formattedDate},{formattedDate},'text/plain',{dto.DialogId},3");

        return dialogContentCsvData.ToString();






        //
        // while (currentDate < endDate)
        // {
        //     var dialogId = DeterministicUuidV7.Generate(currentDate, nameof(DialogEntity));
        //
        //     var contentId1 = DeterministicUuidV7.Generate(currentDate, nameof(Domain.Dialogporten.Domain.Dialogs.Entities.Contents.DialogContent), 1);
        //     var contentId2 = DeterministicUuidV7.Generate(currentDate, nameof(Domain.Dialogporten.Domain.Dialogs.Entities.Contents.DialogContent), 2);
        //
        //     var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
        //     dialogContentCsvData.AppendLine($"{contentId1},{formattedDate},{formattedDate},'text/plain',{dialogId},1");
        //     dialogContentCsvData.AppendLine($"{contentId2},{formattedDate},{formattedDate},'text/plain',{dialogId},3");
        //
        //     currentDate = currentDate.Add(intervalSeconds);
        // }
        //
        // return dialogContentCsvData.ToString();
    }
}
