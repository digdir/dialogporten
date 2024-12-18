using System.Diagnostics;
using System.Globalization;
using System.Text;
using Medo;
using Npgsql;

#pragma warning disable CA1305

try
{
    var connString = Environment.GetEnvironmentVariable("CONN_STRING");
    var intervalSeconds = Environment.GetEnvironmentVariable("INTERVAL_SECONDS");
    var endDateMonths = Environment.GetEnvironmentVariable("NUM_MONTHS");

    if (string.IsNullOrEmpty(connString) || string.IsNullOrEmpty(intervalSeconds) ||
        string.IsNullOrEmpty(endDateMonths))
    {
        Console.WriteLine("Please set the environment variables: CONN_STRING, INTERVAL_SECONDS, and NUM_MONTHS.");
        return;
    }

    var interval = TimeSpan.FromSeconds(int.Parse(intervalSeconds));
    var endDateMonthsInt = int.Parse(endDateMonths);

    const string startDateString = "1986/01/03 00:00:00 +00:00"; // TODO: Parameterize
    var currentDate =
        DateTimeOffset.ParseExact(startDateString, "yyyy/MM/dd HH:mm:ss zzz", CultureInfo.InvariantCulture);
    var endDate = currentDate.AddMonths(endDateMonthsInt);

    var serviceResources = File.ReadAllLines("./service_resources");

    var startTimestamp = Stopwatch.GetTimestamp();

    var dialogsCreated = 0;

    while (currentDate < endDate)
    {
        var monthEndDate = currentDate.AddMonths(1);
        var dialogCsvData = new StringBuilder();
        dialogCsvData.AppendLine(
            "Id,CreatedAt,Deleted,DeletedAt,DueAt,ExpiresAt,ExtendedStatus,ExternalReference,Org,Party,PrecedingProcess,Process,Progress,Revision,ServiceResource,ServiceResourceType,StatusId,VisibleFrom,UpdatedAt");

        List<Guid> dialogIds = [];
        var currentServiceResourceIndex = 0;
        while (currentDate < monthEndDate)
        {
            var dialogId = Uuid7.NewUuid7(currentDate).ToGuid();
            dialogIds.Add(dialogId);

            var serviceResource = serviceResources[currentServiceResourceIndex];
            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            dialogCsvData.AppendLine(
                $"{dialogId},{formattedDate},FALSE,,,,sql-generated,NULL,ttd,partyHere,NULL,NULL,11,{Guid.NewGuid()},{serviceResource},GenericAccessResource,1,,{formattedDate}");

            currentDate = currentDate.Add(interval);
            currentServiceResourceIndex = (currentServiceResourceIndex + 1) % serviceResources.Length;
        }

        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        using var writer = conn.BeginTextImport(
            "COPY \"Dialog\" (\"Id\", \"CreatedAt\", \"Deleted\", \"DeletedAt\", \"DueAt\", \"ExpiresAt\", \"ExtendedStatus\", \"ExternalReference\", \"Org\", \"Party\", \"PrecedingProcess\", \"Process\", \"Progress\", \"Revision\", \"ServiceResource\", \"ServiceResourceType\", \"StatusId\", \"VisibleFrom\", \"UpdatedAt\") FROM STDIN (FORMAT csv, HEADER true, NULL '')");
        writer.Write(dialogCsvData.ToString());

        dialogsCreated += dialogIds.Count;
    }

    var timeItTook = Stopwatch.GetElapsedTime(startTimestamp);
    Console.WriteLine($"Generated {dialogsCreated} dialogs in {timeItTook}");
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
}


// TODO: Re-enable all indexes and constraints
#pragma warning restore CA1305
