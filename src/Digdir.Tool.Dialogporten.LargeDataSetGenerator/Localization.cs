using System.Runtime.InteropServices;
using System.Text;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions.Contents;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class Localization
{
    public const string CopyCommand = """COPY "Localization" ("LanguageCode", "LocalizationSetId", "CreatedAt", "UpdatedAt", "Value") FROM STDIN (FORMAT csv, HEADER false, NULL '')""";

    public static string Generate(DialogTimestamp dto)
    {
        var csvData = new StringBuilder();

        List<Guid> localizationSetIds =
        [
            dto.DialogId,
            DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogTransmission), 1),
            DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogTransmission), 2),
            DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogGuiAction), 1),
            DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogGuiAction), 2),
            DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogGuiAction), 3),
            DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogActivity), Activity.DialogCreatedType),
            DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogActivity), Activity.InformationType),
            DeterministicUuidV7.Generate(dto.Timestamp,
                nameof(Domain.Dialogporten.Domain.Dialogs.Entities.Contents.DialogContent), 1),
            DeterministicUuidV7.Generate(dto.Timestamp,
                nameof(Domain.Dialogporten.Domain.Dialogs.Entities.Contents.DialogContent), 2),
            DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogTransmissionContent), 1),
            DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogTransmissionContent), 2),
            DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogTransmissionContent), 3),
            DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogTransmissionContent), 4)
        ];

        foreach (var localizationSetId in localizationSetIds)
        {
            csvData.AppendLine(
                $"nb,{localizationSetId},{dto.FormattedTimestamp},{dto.FormattedTimestamp},Norsk {Guid.NewGuid().ToString()[..8]}");
            csvData.AppendLine(
                $"en,{localizationSetId},{dto.FormattedTimestamp},{dto.FormattedTimestamp},English {Guid.NewGuid().ToString()[..8]}");
        }

        return csvData.ToString();
    }
}
