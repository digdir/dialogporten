using System.Diagnostics;
using Digdir.Tool.Dialogporten.LargeDataSetGenerator;
using Npgsql;
using Activity = Digdir.Tool.Dialogporten.LargeDataSetGenerator.Activity;

#pragma warning disable CA1305
#pragma warning disable IDE0061

try
{
    Console.WriteLine("Starting large data set generator...");

    var connString = Environment.GetEnvironmentVariable("CONN_STRING");
    var startingDate = DateTimeOffset.Parse(Environment.GetEnvironmentVariable("FROM_DATE")!);
    var endDate = DateTimeOffset.Parse(Environment.GetEnvironmentVariable("TO_DATE")!);
    var dialogAmount = int.Parse(Environment.GetEnvironmentVariable("DIALOG_AMOUNT")!);

    Console.WriteLine($"Connection string: {connString}");
    Console.WriteLine($"Starting date: {startingDate}");
    Console.WriteLine($"End date: {endDate}");
    Console.WriteLine($"Dialog amount: {dialogAmount}");

    var totalDialogCreatedStartTimestamp = Stopwatch.GetTimestamp();

    await using var dataSource = NpgsqlDataSource.Create(connString!);
    var dto = new SeedDatabaseDto(startingDate, endDate, dialogAmount);
    var tasks = new List<Task>();

    void CreateTask(Func<DialogTimestamp, string> generator, string entityName,
        string copyCommand, bool singleLinePerTimestamp = false, int splits = 1)
    {
        for (var i = 0; i < splits; i++)
        {
            var splitIndex = i;
            tasks.Add(Task.Run(async () =>
            {
                await using var dbConnection = await dataSource.OpenConnectionAsync();
                await using var writer = dbConnection.BeginTextImport(copyCommand);

                var startTimestamp = Stopwatch.GetTimestamp();
                foreach (var timestamp in dto.GetDialogTimestamps(splits, splitIndex))
                {
                    var data = generator(timestamp);

                    if (singleLinePerTimestamp)
                    {
                        await writer.WriteLineAsync(data);
                    }
                    else
                    {
                        await writer.WriteAsync(data);
                    }

                    if (timestamp.Counter % 500_000 == 0)
                    {
                        Console.WriteLine(
                            $"Inserted 500k dialogs worth of {entityName} (split {splitIndex + 1}/{splits}), counter at {timestamp.Counter}");
                    }
                }

                Console.WriteLine(
                    $"Inserted {entityName} (split {splitIndex + 1}/{splits}) in {Stopwatch.GetElapsedTime(startTimestamp)}");
            }));
        }
    }

    // Split Localizations, 28 lines per dialog
    CreateTask(Localization.Generate, "localizations", Localization.CopyCommand, splits: 14);

    // Split LocalizationSets, 14 lines per dialog
    CreateTask(LocalizationSet.Generate, "localization sets", LocalizationSet.CopyCommand, splits: 7);

    // Split AttachmentUrls, 6 lines per dialog
    CreateTask(AttachmentUrl.Generate, "attachment URLs", AttachmentUrl.CopyCommand, splits: 3);

    // Split Actors, 5 lines per dialog
    CreateTask(Actor.Generate, "actors", Actor.CopyCommand, splits: 2);

    // Split TransmissionContent, 4 lines per dialog
    CreateTask(TransmissionContent.Generate, "transmission content", TransmissionContent.CopyCommand, splits: 2);

    // No split, 2-3 lines per dialog
    CreateTask(DialogContent.Generate, "dialog content", DialogContent.CopyCommand);
    CreateTask(Transmission.Generate, "transmissions", Transmission.CopyCommand);
    CreateTask(GuiAction.Generate, "dialog gui actions", GuiAction.CopyCommand);
    CreateTask(Activity.Generate, "activities", Activity.CopyCommand);
    CreateTask(Attachment.Generate, "attachments", Attachment.CopyCommand);
    CreateTask(SearchTags.Generate, "search tags", SearchTags.CopyCommand);

    // Single line per dialog
    CreateTask(Dialog.Generate, "dialogs", Dialog.CopyCommand, singleLinePerTimestamp: true);
    CreateTask(SeenLog.Generate, "seen logs", SeenLog.CopyCommand, singleLinePerTimestamp: true);
    CreateTask(EndUserContext.Generate, "end user contexts", EndUserContext.CopyCommand,
        singleLinePerTimestamp: true);

    await Task.WhenAll(tasks);

    Console.WriteLine(string.Empty);
    Console.WriteLine(string.Empty);

    var timeItTook = Stopwatch.GetElapsedTime(totalDialogCreatedStartTimestamp);
    Console.WriteLine($"Generates {dialogAmount} in {timeItTook}");
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
}

// TODO: Re-enable all indexes and constraints
#pragma warning restore CA1305
