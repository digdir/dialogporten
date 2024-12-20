using System.Text;
using Medo;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

public static class Transmission
{
    private const string CsvHeader = "Id,CreatedAt,AuthorizationAttribute,ExtendedType,TypeId,DialogId,RelatedTransmissionId";

    public const string CopyCommand = "COPY \"DialogTransmission\" (\"Id\", \"CreatedAt\", \"AuthorizationAttribute\", \"ExtendedType\", \"TypeId\", \"DialogId\", \"RelatedTransmissionId\") FROM STDIN (FORMAT csv, HEADER true, NULL '')";

    public static (List<Guid> transmissionIds, string transmissionCsvData) Generate(List<Guid> dialogIds,
        DateTimeOffset currentDate, int intervalSeconds)
    {
        var transmissionCsvData = new StringBuilder();
        transmissionCsvData.AppendLine(CsvHeader);

        List<Guid> transmissionIds = [];
        foreach (var dialogId in dialogIds)
        {
            var transmissionId1 = Uuid7.NewUuid7(currentDate).ToGuid();
            var transmissionId2 = Uuid7.NewUuid7(currentDate).ToGuid();

            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            transmissionCsvData.AppendLine($"{transmissionId1},{formattedDate},NULL,NULL,1,{dialogId},");
            transmissionCsvData.AppendLine(
                $"{transmissionId2},{formattedDate},NULL,NULL,2,{dialogId},{transmissionId1}");

            transmissionIds.Add(transmissionId1);
            transmissionIds.Add(transmissionId2);

            currentDate = currentDate.AddSeconds(intervalSeconds);
        }

        return (transmissionIds, transmissionCsvData.ToString());
    }
}


public static class AttachmentUrl
{
    private const string CsvHeader = "Id,CreatedAt,MediaType,Url,ConsumerTypeId,AttachmentId";

    public const string CopyCommand = "COPY \"AttachmentUrl\" (\"Id\", \"CreatedAt\", \"MediaType\", \"Url\", \"ConsumerTypeId\", \"AttachmentId\") FROM STDIN (FORMAT csv, HEADER true, NULL '')";

    public static string Generate(List<Guid> attachmentIds,
        DateTimeOffset currentDate, int intervalSeconds)
    {
        var attachmentUrlCsvData = new StringBuilder();
        attachmentUrlCsvData.AppendLine(CsvHeader);

        foreach (var attachmentId in attachmentIds)
        {
            var attachmentUrlId1 = Uuid7.NewUuid7(currentDate).ToGuid();
            var attachmentUrlId2 = Uuid7.NewUuid7(currentDate).ToGuid();

            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            attachmentUrlCsvData.AppendLine($"{attachmentUrlId1},{formattedDate},'text/plain','https://digdir.apps.tt02.altinn.no/',1,{attachmentId}");
            attachmentUrlCsvData.AppendLine($"{attachmentUrlId2},{formattedDate},'text/plain','https://digdir.apps.tt02.altinn.no/',2,{attachmentId}");

            currentDate = currentDate.AddSeconds(intervalSeconds);
        }

        return attachmentUrlCsvData.ToString();
    }
}


public static class Actor
{
    private const string CsvHeader = "Id,ActorId,ActorTypeId,ActorName,Discriminator,ActivityId,DialogSeenLogId,TransmissionId,CreatedAt,UpdatedAt,LabelAssignmentLogId";

    public const string CopyCommand = "COPY \"Actor\" (\"Id\", \"ActorId\", \"ActorTypeId\", \"ActorName\", \"Discriminator\", \"ActivityId\", \"DialogSeenLogId\", \"TransmissionId\", \"CreatedAt\", \"UpdatedAt\", \"LabelAssignmentLogId\") FROM STDIN (FORMAT csv, HEADER true, NULL '')";

    public static string Generate(List<Guid> dialogActivityIds, List<Guid> dialogSeenLogIds, List<Guid> transmissionIds, DateTimeOffset currentDate, int _)
    {
        var actorCsvData = new StringBuilder();
        actorCsvData.AppendLine(CsvHeader);

        foreach (var dialogActivityId in dialogActivityIds)
        {
            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            actorCsvData.AppendLine($"{dialogActivityId},NULL,1,'ActorName','DialogActivityPerformedByActor',{dialogActivityId},,,{formattedDate},{formattedDate},");
        }

        foreach (var dialogSeenLogId in dialogSeenLogIds)
        {
            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            actorCsvData.AppendLine($"{dialogSeenLogId},NULL,1,'ActorName','DialogSeenLogSeenByActor',,{dialogSeenLogId},,{formattedDate},{formattedDate},");
        }

        foreach (var transmissionId in transmissionIds)
        {
            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            actorCsvData.AppendLine($"{transmissionId},NULL,1,'ActorName','DialogTransmissionSenderActor',,,{transmissionId},{formattedDate},{formattedDate},");
        }

        return actorCsvData.ToString();
    }
}

public static class LocalizationSet
{
    private const string CsvHeader = "Id,CreatedAt,Discriminator,AttachmentId,GuiActionId,ActivityId,DialogContentId,TransmissionContentId";

    public const string CopyCommand = "COPY \"LocalizationSet\" (\"Id\", \"CreatedAt\", \"Discriminator\", \"AttachmentId\", \"GuiActionId\", \"ActivityId\", \"DialogContentId\", \"TransmissionContentId\") FROM STDIN (FORMAT csv, HEADER true, NULL '')";

    public static (List<Guid> localizationSetIds, string localizationSetCsvData) Generate(List<Guid> attachmentIds, List<Guid> guiActionIds, List<Guid> dialogActivityIds, List<Guid> dialogContentIds, List<Guid> transmissionContentIds, DateTimeOffset currentDate, int _)
    {
        var localizationSetCsvData = new StringBuilder();
        localizationSetCsvData.AppendLine(CsvHeader);

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


public static class Localization
{
    private const string CsvHeader = "LanguageCode,LocalizationSetId,CreatedAt,UpdatedAt,Value";

    public const string CopyCommand = "COPY \"Localization\" (\"LanguageCode\", \"LocalizationSetId\", \"CreatedAt\", \"UpdatedAt\", \"Value\") FROM STDIN (FORMAT csv, HEADER true, NULL '')";

    public static string Generate(List<Guid> localizationSetIds, DateTimeOffset currentDate)
    {
        var localizationCsvData = new StringBuilder();
        localizationCsvData.AppendLine(CsvHeader);

        foreach (var localizationSetId in localizationSetIds)
        {
            // This is wrong, the date cannot be the same for all rows.
            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            localizationCsvData.AppendLine($"nb,{localizationSetId},{formattedDate},{formattedDate},Norsk {Guid.NewGuid().ToString()[..8]}");
            localizationCsvData.AppendLine($"en,{localizationSetId},{formattedDate},{formattedDate},English {Guid.NewGuid().ToString()[..8]}");
        }

        return localizationCsvData.ToString();
    }
}
