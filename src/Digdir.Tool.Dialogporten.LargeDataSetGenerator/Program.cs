using System.Diagnostics;
using System.Globalization;
using System.Text;
using Medo;
using Npgsql;

#pragma warning disable CA1305

try
{
    Console.WriteLine("Starting large data set generator...");

    var connString = Environment.GetEnvironmentVariable("CONN_STRING");
    var intervalSeconds = int.Parse(Environment.GetEnvironmentVariable("INTERVAL_SECONDS")!);
    var batchNumDays = int.Parse(Environment.GetEnvironmentVariable("BATCH_NUM_DAYS")!);
    var startingYear = int.Parse(Environment.GetEnvironmentVariable("STARTING_YEAR")!);
    var totalNumMonths = int.Parse(Environment.GetEnvironmentVariable("TOTAL_NUM_MONTHS")!);


    var startDateString = $"{startingYear}/01/01 00:00:00 +00:00";
    var currentDate = DateTimeOffset.ParseExact(
        startDateString, "yyyy/MM/dd HH:mm:ss zzz",
        CultureInfo.InvariantCulture);

    var endDate = currentDate.AddMonths(totalNumMonths);

    var serviceResources = File.ReadAllLines("./service_resources");

    var totalDialogCreatedStartTimestamp = Stopwatch.GetTimestamp();

    var dialogsCreated = 0;

    while (currentDate < endDate)
    {
        var batchEndDate = currentDate.AddDays(batchNumDays);
        var dialogCsvData = new StringBuilder();
        dialogCsvData.AppendLine(
            "Id,CreatedAt,Deleted,DeletedAt,DueAt,ExpiresAt,ExtendedStatus,ExternalReference,Org,Party,PrecedingProcess,Process,Progress,Revision,ServiceResource,ServiceResourceType,StatusId,VisibleFrom,UpdatedAt");

        List<Guid> dialogIds = [];
        var currentServiceResourceIndex = 0;
        var dialogBatchStartTimestamp = Stopwatch.GetTimestamp();

        while (currentDate < batchEndDate)
        {
            var dialogId = Uuid7.NewUuid7(currentDate).ToGuid();
            dialogIds.Add(dialogId);

            var serviceResource = serviceResources[currentServiceResourceIndex];
            var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            dialogCsvData.AppendLine(
                $"{dialogId},{formattedDate},FALSE,,,,sql-generated,NULL,ttd,partyHere,NULL,NULL,11,{Guid.NewGuid()},{serviceResource},GenericAccessResource,1,,{formattedDate}");

            currentDate = currentDate.AddSeconds(intervalSeconds);
            currentServiceResourceIndex = (currentServiceResourceIndex + 1) % serviceResources.Length;
        }

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            using var writer = conn.BeginTextImport(
                "COPY \"Dialog\" (\"Id\", \"CreatedAt\", \"Deleted\", \"DeletedAt\", \"DueAt\", \"ExpiresAt\", \"ExtendedStatus\", \"ExternalReference\", \"Org\", \"Party\", \"PrecedingProcess\", \"Process\", \"Progress\", \"Revision\", \"ServiceResource\", \"ServiceResourceType\", \"StatusId\", \"VisibleFrom\", \"UpdatedAt\") FROM STDIN (FORMAT csv, HEADER true, NULL '')");
            writer.Write(dialogCsvData.ToString());
        }

        dialogsCreated += dialogIds.Count;
        Console.WriteLine($"Inserted {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(dialogBatchStartTimestamp)}");


        // Dialog Content:
        var dialogContentStartTimestamp = Stopwatch.GetTimestamp();
        var dialogContentCsvData = new StringBuilder();
        dialogContentCsvData.AppendLine("Id,CreatedAt,UpdatedAt,MediaType,DialogId,TypeId");

        List<Guid> dialogContentIds = [];
        foreach (var dialogId in dialogIds)
        {
            var createdAt = currentDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            var contentId1 = Uuid7.NewUuid7(currentDate).ToGuid();
            var contentId2 = Uuid7.NewUuid7(currentDate).ToGuid();

            dialogContentCsvData.AppendLine($"{contentId1},{createdAt},{createdAt},'text/plain',{dialogId},1");
            dialogContentCsvData.AppendLine($"{contentId2},{createdAt},{createdAt},'text/plain',{dialogId},3");

            dialogContentIds.Add(contentId1);
            dialogContentIds.Add(contentId2);
        }

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            using var contentWriter = conn.BeginTextImport(
                "COPY \"DialogContent\" (\"Id\", \"CreatedAt\", \"UpdatedAt\", \"MediaType\", \"DialogId\", \"TypeId\") FROM STDIN (FORMAT csv, HEADER true, NULL '')");
            contentWriter.Write(dialogContentCsvData.ToString());
        }

        Console.WriteLine($"Inserted {dialogContentIds.Count} dialog content for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(dialogContentStartTimestamp)}");




        Console.WriteLine(string.Empty);
    }

    var timeItTook = Stopwatch.GetElapsedTime(totalDialogCreatedStartTimestamp);
    Console.WriteLine($"Generated {dialogsCreated} dialogs in {timeItTook}");
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
}

// TODO: Re-enable all indexes and constraints
#pragma warning restore CA1305
