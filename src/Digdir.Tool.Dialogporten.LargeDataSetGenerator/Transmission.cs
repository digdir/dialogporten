using System.Text;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class Transmission
{
    public const string CopyCommand = """COPY "DialogTransmission" ("Id", "CreatedAt", "AuthorizationAttribute", "ExtendedType", "TypeId", "DialogId", "RelatedTransmissionId") FROM STDIN (FORMAT csv, HEADER false, NULL '')""";

    public static string Generate(DialogTimestamp dto)
    {
        var transmissionCsvData = new StringBuilder();

        var transmissionId1 = DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogTransmission), 1);
        var transmissionId2 = DeterministicUuidV7.Generate(dto.Timestamp, nameof(DialogTransmission), 2);

        transmissionCsvData.AppendLine($"{transmissionId1},{dto.FormattedTimestamp},NULL,NULL,1,{dto.DialogId},");
        transmissionCsvData.AppendLine($"{transmissionId2},{dto.FormattedTimestamp},NULL,NULL,2,{dto.DialogId},{transmissionId1}");

        return transmissionCsvData.ToString();
    }
}
