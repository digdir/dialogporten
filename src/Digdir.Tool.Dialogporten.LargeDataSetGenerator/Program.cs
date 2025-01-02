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

    // await using var dataSource = NpgsqlDataSource.Create(connString!);
    var dto = new SeedDatabaseDto(startingDate, endDate, dialogAmount);
    var tasks = new List<Task>();

    const int taskRetryDelayInMs = 10000;
    const int taskRetryLimit = 1000;

    void CreateTask(Func<DialogTimestamp, string> generator, string entityName,
        string copyCommand, bool singleLinePerTimestamp = false, int splits = 1)
    {
        for (var i = 0; i < splits; i++)
        {
            var splitIndex = i;
            tasks.Add(Task.Run(async () =>
            {
                var startTimestamp = Stopwatch.GetTimestamp();
                var counter = 0;
                // var currentTaskHasFailed = false;
                do
                {
                    try
                    {
                        // await using var dbConnection = await dataSource.OpenConnectionAsync();

                        var dbConnection = new NpgsqlConnection(connString);
                        await dbConnection.OpenAsync();
                        var transaction = await dbConnection.BeginTransactionAsync();
                        await using var writer = dbConnection.BeginTextImport(copyCommand);

                        try
                        {
                            const int logThreshold = 500_000;
                            var splitLogThreshold = logThreshold / splits;

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

                                // if (!currentTaskHasFailed)
                                // {
                                //     var random = new Random();
                                //
                                //     if (random.Next(10) == 0) // 1 out of 10 chance
                                //     {
                                //         currentTaskHasFailed = true;
                                //         throw new ArgumentNullException("Randomly thrown exception");
                                //     }
                                // }

                                if (timestamp.Counter % logThreshold == 0)
                                {
                                    Console.WriteLine(
                                        $"Inserted {splitLogThreshold} dialogs worth of {entityName} (split {splitIndex + 1}/{splits}), counter at {timestamp.Counter}");
                                }
                            }

                            // await writer.FlushAsync();
                            await transaction.CommitAsync();

                            counter = taskRetryLimit + 20000;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine();
                            Console.WriteLine("====================================");
                            Console.WriteLine($"Insert for table {entityName} failed (split {splitIndex + 1}/{splits}), retrying in {taskRetryDelayInMs}ms");
                            Console.WriteLine(e.Message);
                            Console.WriteLine(e.StackTrace);
                            Console.WriteLine("====================================");
                            Console.WriteLine();
                            await transaction.RollbackAsync();
                            await dbConnection.CloseAsync();

                            // await dbConnection.DisposeAsync().ConfigureAwait(false);
                            counter++;
                            Thread.Sleep(taskRetryDelayInMs);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine();
                        Console.WriteLine("====================================");
                        Console.WriteLine($"Database setup failed, either connection or transaction, retrying in {taskRetryDelayInMs}ms");
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                        Console.WriteLine("====================================");
                        Console.WriteLine();
                        counter++;
                        Thread.Sleep(taskRetryDelayInMs);
                    }
                }
                while (counter < taskRetryLimit);

                Console.WriteLine(
                    $"Inserted {entityName} (split {splitIndex + 1}/{splits}) in {Stopwatch.GetElapsedTime(startTimestamp)}");
            }));
        }
    }

    // Split Localizations, 28 lines per dialog
    CreateTask(Localization.Generate, "localizations", Localization.CopyCommand, splits: 3);

    // Split LocalizationSets, 14 lines per dialog
    CreateTask(LocalizationSet.Generate, "localization sets", LocalizationSet.CopyCommand, splits: 3);

    // Split AttachmentUrls, 6 lines per dialog
    CreateTask(AttachmentUrl.Generate, "attachment URLs", AttachmentUrl.CopyCommand, splits: 2);

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
    Console.WriteLine($"Generated {dialogAmount} in {timeItTook}");
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    Console.Error.WriteLine(ex.StackTrace);
}

// TODO: Re-enable all indexes and constraints
#pragma warning restore CA1305
