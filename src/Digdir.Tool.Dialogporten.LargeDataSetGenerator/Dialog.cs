using System.Text;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class Dialog
{
    private static readonly string[] ServiceResources = File.ReadAllLines("./service_resources");
    public const string CopyCommand = "COPY \"Dialog\" (\"Id\", \"CreatedAt\", \"Deleted\", \"DeletedAt\", \"DueAt\", \"ExpiresAt\", \"ExtendedStatus\", \"ExternalReference\", \"Org\", \"Party\", \"PrecedingProcess\", \"Process\", \"Progress\", \"Revision\", \"ServiceResource\", \"ServiceResourceType\", \"StatusId\", \"VisibleFrom\", \"UpdatedAt\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    // public int EntitiesPerBatch =>
    public static string Generate(DialogTimestamp dto)
    {
        // var dialogId = DeterministicUuidV7.Generate(timestamp, nameof(DialogEntity));
        var formattedDate = dto.Timestamp.ToString("yyyy-MM-dd HH:mm:ss zzz");

        // TODO: ServiceResource/Party round robin
        return $"{dto.DialogId},{formattedDate},FALSE,,,sql-generated,NULL,ttd,partyHere,NULL,NULL,11,{Guid.NewGuid()},{ServiceResources[0]},GenericAccessResource,1,,{formattedDate}";
        // var dialogCsvData = new StringBuilder();
        //
        // var currentServiceResourceIndex = 0;
        //
        // var currentDate = startDate;
        // while (currentDate < endDate)
        // {
        //     var dialogId = DeterministicUuidV7.Generate(currentDate, nameof(DialogEntity));
        //
        //     var serviceResource = ServiceResources[currentServiceResourceIndex];
        //     var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
        //     dialogCsvData.AppendLine(
        //         $"{dialogId},{formattedDate},FALSE,,,,sql-generated,NULL,ttd,partyHere,NULL,NULL,11,{Guid.NewGuid()},{serviceResource},GenericAccessResource,1,,{formattedDate}");
        //
        //     currentDate = currentDate.Add(intervalSeconds);
        //     currentServiceResourceIndex = (currentServiceResourceIndex + 1) % ServiceResources.Length;
        // }
        //
        // return dialogCsvData.ToString();
    }
}
