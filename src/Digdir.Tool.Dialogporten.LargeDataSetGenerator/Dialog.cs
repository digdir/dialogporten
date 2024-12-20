using System.Text;
using Medo;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class Dialog
{
    private static readonly string[] ServiceResources = File.ReadAllLines("./service_resources");
    private const string CsvHeader = "Id,CreatedAt,Deleted,DeletedAt,DueAt,ExpiresAt,ExtendedStatus,ExternalReference,Org,Party,PrecedingProcess,Process,Progress,Revision,ServiceResource,ServiceResourceType,StatusId,VisibleFrom,UpdatedAt";
    public const string CopyCommand = "COPY \"Dialog\" (\"Id\", \"CreatedAt\", \"Deleted\", \"DeletedAt\", \"DueAt\", \"ExpiresAt\", \"ExtendedStatus\", \"ExternalReference\", \"Org\", \"Party\", \"PrecedingProcess\", \"Process\", \"Progress\", \"Revision\", \"ServiceResource\", \"ServiceResourceType\", \"StatusId\", \"VisibleFrom\", \"UpdatedAt\") FROM STDIN (FORMAT csv, HEADER false, NULL '')";

    public static (List<Guid> dialogIds, string dialogCsvData) Generate(DateTimeOffset startDate, DateTimeOffset endDate, int intervalSeconds)
    {
        List<Guid> dialogIds = [];

        var dialogCsvData = new StringBuilder();

        // dialogCsvData.AppendLine(CsvHeader);

        var currentServiceResourceIndex = 0;

        var currentDate = startDate;
        while (currentDate < endDate)
        {
            var dialogId = Uuid7.NewUuid7(currentDate).ToGuid();

            var serviceResource = ServiceResources[currentServiceResourceIndex];
            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            dialogCsvData.AppendLine(
                $"{dialogId},{formattedDate},FALSE,,,,sql-generated,NULL,ttd,partyHere,NULL,NULL,11,{Guid.NewGuid()},{serviceResource},GenericAccessResource,1,,{formattedDate}");

            currentDate = currentDate.AddSeconds(intervalSeconds);
            currentServiceResourceIndex = (currentServiceResourceIndex + 1) % ServiceResources.Length;

            dialogIds.Add(dialogId);
        }

        return (dialogIds, dialogCsvData.ToString());
    }
}
