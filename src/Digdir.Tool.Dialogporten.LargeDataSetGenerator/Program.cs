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

    await using var dialogConnection = await dataSource.OpenConnectionAsync();
    await using var dialogWriter = dialogConnection.BeginTextImport(Dialog.CopyCommand);

    await using var dialogContentConnection = await dataSource.OpenConnectionAsync();
    await using var dialogContentWriter = dialogContentConnection.BeginTextImport(DialogContent.CopyCommand);

    await using var guiActionConnection = await dataSource.OpenConnectionAsync();
    await using var guiActionWriter = guiActionConnection.BeginTextImport(GuiAction.CopyCommand);

    await using var endUserContextConnection = await dataSource.OpenConnectionAsync();
    await using var endUserContextWriter = endUserContextConnection.BeginTextImport(EndUserContext.CopyCommand);

    await using var seenLogConnection = await dataSource.OpenConnectionAsync();
    await using var seenLogWriter = seenLogConnection.BeginTextImport(SeenLog.CopyCommand);

    await using var searchTagConnection = await dataSource.OpenConnectionAsync();
    await using var searchTagWriter = searchTagConnection.BeginTextImport(SearchTags.CopyCommand);

    await using var transmissionConnection = await dataSource.OpenConnectionAsync();
    await using var transmissionWriter = transmissionConnection.BeginTextImport(Transmission.CopyCommand);

    await using var transmissionContentConnection = await dataSource.OpenConnectionAsync();
    await using var transmissionContentWriter = transmissionContentConnection.BeginTextImport(TransmissionContent.CopyCommand);

    await using var activityConnection = await dataSource.OpenConnectionAsync();
    await using var activityWriter = activityConnection.BeginTextImport(Activity.CopyCommand);

    await using var attachmentConnection = await dataSource.OpenConnectionAsync();
    await using var attachmentWriter = attachmentConnection.BeginTextImport(Attachment.CopyCommand);

    await using var attachmentUrlConnection = await dataSource.OpenConnectionAsync();
    await using var attachmentUrlWriter = attachmentUrlConnection.BeginTextImport(AttachmentUrl.CopyCommand);

    await using var actorConnection = await dataSource.OpenConnectionAsync();
    await using var actorWriter = actorConnection.BeginTextImport(Actor.CopyCommand);

    await using var localizationSetConnection = await dataSource.OpenConnectionAsync();
    await using var localizationSetWriter = localizationSetConnection.BeginTextImport(LocalizationSet.CopyCommand);

    await using var localizationConnection = await dataSource.OpenConnectionAsync();
    await using var localizationWriter = localizationConnection.BeginTextImport(Localization.CopyCommand);

    var dto = new SeedDatabaseDto(startingDate, endDate, dialogAmount);

    var tasks = new List<Task>();

    Task CreateTask(Func<DialogTimestamp, string> generator, Func<string, Task> writer, string entityName)
        => Task.Run(async () =>
        {
            var startTimestamp = Stopwatch.GetTimestamp();
            foreach (var timestamp in dto.GetDialogTimestamps)
            {
                var data = generator(timestamp);
                if (data == string.Empty)
                {
                    continue;
                }

                await writer(data);

                if (timestamp.Counter % 200_000 == 0)
                {
                    Console.WriteLine($"Inserted 200k dialogs worth of {entityName}, counter at {timestamp.Counter}");
                }
            }
            Console.WriteLine($"Inserted {entityName} in {Stopwatch.GetElapsedTime(startTimestamp)}");
        });

    tasks.Add(CreateTask(Localization.GenerateOdd, localizationWriter.WriteAsync, "localizations (odd)"));
    tasks.Add(CreateTask(Localization.GenerateEven, localizationWriter.WriteAsync, "localizations (even)"));
    tasks.Add(CreateTask(LocalizationSet.GenerateOdd, localizationSetWriter.WriteAsync, "localization sets (odd)"));
    tasks.Add(CreateTask(LocalizationSet.GenerateEven, localizationSetWriter.WriteAsync, "localization sets (odd)"));
    tasks.Add(CreateTask(DialogContent.Generate, dialogContentWriter.WriteAsync, "dialog content"));
    tasks.Add(CreateTask(Transmission.Generate, transmissionWriter.WriteAsync, "transmissions"));
    tasks.Add(CreateTask(GuiAction.Generate, guiActionWriter.WriteAsync, "dialog gui actions"));
    tasks.Add(CreateTask(EndUserContext.Generate, endUserContextWriter.WriteLineAsync, "end user contexts"));
    tasks.Add(CreateTask(TransmissionContent.Generate, transmissionContentWriter.WriteAsync, "transmission content"));
    tasks.Add(CreateTask(Activity.Generate, activityWriter.WriteAsync, "activities"));
    tasks.Add(CreateTask(Attachment.Generate, attachmentWriter.WriteAsync, "attachments"));
    tasks.Add(CreateTask(Dialog.Generate, dialogWriter.WriteLineAsync, "dialogs"));
    tasks.Add(CreateTask(AttachmentUrl.Generate, attachmentUrlWriter.WriteAsync, "attachment URLs"));
    tasks.Add(CreateTask(Actor.Generate, actorWriter.WriteAsync, "actors"));
    tasks.Add(CreateTask(SeenLog.Generate, seenLogWriter.WriteLineAsync, "seen logs"));
    tasks.Add(CreateTask(SearchTags.Generate, searchTagWriter.WriteAsync, "search tags"));

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
