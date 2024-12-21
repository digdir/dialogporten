using System.Text;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class Activity
{
    public const string CopyCommand = """COPY "DialogActivity" ("Id", "CreatedAt", "ExtendedType", "TypeId", "TransmissionId", "DialogId") FROM STDIN (FORMAT csv, HEADER false, NULL '')""";

    public const int DialogCreatedType = 1;
    public const int InformationType = 2;

    public const int EntitiesPerParent = 2;
    // public const int EntitiesPerBatch = EntitiesPerParent * Dialog.EntitiesPerBatch;

    public static string Generate(DialogTimestamp dto)
    {
        var activityCsvData = new StringBuilder();

        var activityId1 = DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogActivity), DialogCreatedType);
        var activityId2 = DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogActivity), InformationType);

        activityCsvData.AppendLine($"{activityId1},{dto.FormattedTimestamp},NULL,1,,{dto.DialogId}"); // Type DialogCreated
        activityCsvData.AppendLine($"{activityId2},{dto.FormattedTimestamp},NULL,3,,{dto.DialogId}"); // Type Information

        return activityCsvData.ToString();
    }
}
