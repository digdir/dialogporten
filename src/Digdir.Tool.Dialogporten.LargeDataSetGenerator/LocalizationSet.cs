using System.Text;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class LocalizationSet
{
    private const string CsvHeader = "Id,CreatedAt,Discriminator,AttachmentId,GuiActionId,ActivityId,DialogContentId,TransmissionContentId";

    public const string CopyCommand = "COPY \"LocalizationSet\" (\"Id\", \"CreatedAt\", \"Discriminator\", \"AttachmentId\", \"GuiActionId\", \"ActivityId\", \"DialogContentId\", \"TransmissionContentId\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public static (List<Guid> localizationSetIds, string localizationSetCsvData) Generate(List<Guid> attachmentIds, List<Guid> guiActionIds, List<Guid> dialogActivityIds, List<Guid> dialogContentIds, List<Guid> transmissionContentIds, DateTimeOffset currentDate, int _)
    {
        var localizationSetCsvData = new StringBuilder();
        // localizationSetCsvData.AppendLine(CsvHeader);

        // TODO: This is wrong, the date cannot be the same for all rows.
        var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");

        foreach (var attachmentId in attachmentIds)
        {
            localizationSetCsvData.AppendLine($"{attachmentId},{formattedDate},'AttachmentDisplayName',{attachmentId},,,,");
        }

        foreach (var guiActionId in guiActionIds)
        {
            localizationSetCsvData.AppendLine($"{guiActionId},{formattedDate},'DialogGuiActionTitle',,{guiActionId},,,");
        }

        foreach (var dialogActivityId in dialogActivityIds)
        {
            localizationSetCsvData.AppendLine($"{dialogActivityId},{formattedDate},'DialogActivityDescription',,,{dialogActivityId},,");
        }

        foreach (var dialogContentId in dialogContentIds)
        {
            localizationSetCsvData.AppendLine($"{dialogContentId},{formattedDate},'DialogContentValue',,,,{dialogContentId},");
        }

        foreach (var transmissionContentId in transmissionContentIds)
        {
            localizationSetCsvData.AppendLine($"{transmissionContentId},{formattedDate},'DialogTransmissionContentValue',,,,,{transmissionContentId}");
        }

        var localizationSetIds =
            attachmentIds
                .Concat(guiActionIds)
                .Concat(dialogActivityIds)
                .Concat(dialogContentIds)
                .Concat(transmissionContentIds)
                .ToList();

        return (localizationSetIds, localizationSetCsvData.ToString());
    }
}
