using System.Diagnostics;
using System.Globalization;
using System.Text;
using Digdir.Tool.Dialogporten.LargeDataSetGenerator;
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

    var totalDialogCreatedStartTimestamp = Stopwatch.GetTimestamp();

    var dialogsCreated = 0;

    while (currentDate < endDate)
    {
        var batchEndDate = currentDate.AddDays(batchNumDays);

        Console.WriteLine($"Generating dialogs from {currentDate} to {batchEndDate}...");
        var dialogStartTimestamp = Stopwatch.GetTimestamp();

        var (dialogIds, dialogCsvData) = Dialog.Generate(currentDate, batchEndDate, intervalSeconds);

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            using var writer = conn.BeginTextImport(Dialog.CopyCommand);
            writer.Write(dialogCsvData);
        }

        dialogsCreated += dialogIds.Count;
        Console.WriteLine($"Inserted {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(dialogStartTimestamp)}");
        Console.WriteLine(string.Empty);


        // Dialog Content:
        var dialogContentStartTimestamp = Stopwatch.GetTimestamp();
        var (dialogContentIds, dialogContentCsvData) = DialogContent.Generate(dialogIds, currentDate, intervalSeconds);

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            using var contentWriter = conn.BeginTextImport(DialogContent.CopyCommand);
            contentWriter.Write(dialogContentCsvData);
        }

        Console.WriteLine($"Inserted {dialogContentIds.Count} dialog content for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(dialogContentStartTimestamp)}");

        // Dialog Gui Action:
        var dialogGuiActionStartTimestamp = Stopwatch.GetTimestamp();
        var (dialogGuiActionIds, dialogGuiActionCsvData) = GuiAction.Generate(dialogIds, currentDate, intervalSeconds);

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();

            using var guiActionWriter = conn.BeginTextImport(GuiAction.CopyCommand);
            guiActionWriter.Write(dialogGuiActionCsvData);
        }

        Console.WriteLine($"Inserted {dialogGuiActionIds.Count} dialog gui actions for {dialogIds.Count} dialogs... it took {Stopwatch.GetElapsedTime(dialogGuiActionStartTimestamp)}");



        currentDate = batchEndDate;
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
